using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MTCG.HTTP;
using MTCG.Interfaces;
using MTCG.Logic;
using MTCG.Models;

namespace MTCG.Endpoints
{
    public class BattlesEndpoint : IHttpEndpoint
    {
        // Thread communication
        private static ConcurrentQueue<(User, int)> _battleQueue = new ConcurrentQueue<(User, int)>();
        private static ConcurrentDictionary<int, string> _battleResults = new ConcurrentDictionary<int, string>();
        private static object lockObject = new();

        private readonly UserService _userService = UserService.Instance;
        private readonly BattleService _battleService = new BattleService();

        public (int, string?) HandleRequest(TcpClient client, HTTPHeader headers, string? body)
        {
            // Check if user is authorized
            string token = HeaderHelper.GetTokenFromHeader(headers)!;
            User? user = _userService.GetUserByToken(token);

            if (user == null)
            {
                return (403, "User not authorized!");
            }

            // User wants to start battle
            if (headers.Method == "POST")
            {
                // Start battle if there is already (at least) one other player waiting
                if (_battleQueue.TryDequeue(out var otherOne))
                {
                    // TODO: Is there a better way to dequeue a tuple?
                    (User otherUser, int otherThread) = otherOne;

                    string battleLog = _battleService.StartBattle(user, otherUser);

                    // Inform other thread about the result of the battle
                    lock (lockObject)
                    {
                        _battleResults[otherThread] = battleLog;
                        Monitor.PulseAll(lockObject);
                    }

                    return (200, battleLog);
                }
                else
                {
                    // Wait till another user joins the queue and triggers the start of the battle, then wait to be messaged by the thread handling the 2nd user
                    _battleQueue.Enqueue((user, Thread.CurrentThread.ManagedThreadId));

                    lock (lockObject)
                    {
                        while (!_battleResults.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                        {
                            Monitor.Wait(lockObject);
                        }

                        // Remove key from dictionary (so that it's clean again if this thread immediately serves another client)
                        string battleLog = _battleResults[Thread.CurrentThread.ManagedThreadId];

                        // https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2.remove?view=net-9.0
                        _battleResults.Remove(Thread.CurrentThread.ManagedThreadId, out _); // For some reason, the function doesn't work without the second parameter

                        return (200, battleLog);
                    }
                }
            }

            return (405, "Method Not Allowed");
        }
    }
}

