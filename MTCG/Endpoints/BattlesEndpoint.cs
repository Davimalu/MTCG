﻿using MTCG.HTTP;
using MTCG.Interfaces;
using MTCG.Interfaces.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Logic;
using MTCG.Models;
using System.Collections.Concurrent;
using System.Net.Sockets;
using MTCG.Models.Enums;

namespace MTCG.Endpoints
{
    public class BattlesEndpoint : IHttpEndpoint
    {
        // Thread communication
        private static ConcurrentQueue<(User, int)> _battleQueue = new ConcurrentQueue<(User, int)>();
        private static ConcurrentDictionary<int, string?> _battleResults = new ConcurrentDictionary<int, string?>();
        private static object _localLock = new();

        private readonly IUserService _userService = UserService.Instance;
        private readonly IEventService _eventService = new EventService();
        private readonly IBattleService _battleService = new BattleService();
        private readonly IHttpHeaderService _httpHeaderService = new HttpHeaderService();

        public BattlesEndpoint()
        {

        }

        #region DependencyInjection
        public BattlesEndpoint(IUserService userService, IBattleService battleService, IHttpHeaderService httpHeaderService)
        {
            _userService = userService;
            _battleService = battleService;
            _httpHeaderService = httpHeaderService;
        }
        #endregion

        public (int, string?) HandleRequest(TcpClient? client, HttpHeader headers, string? body)
        {
            // Check if user is authorized
            string token = _httpHeaderService.GetTokenFromHeader(headers)!;
            User? user = _userService.GetUserByToken(token);

            if (user == null)
            {
                return (401, "User not authorized");
            }

            switch (headers.Method)
            {
                case "POST":
                    return HandleStartBattle(user);
                default:
                    return (405, "Method Not Allowed");
            }
        }

        private (int, string?) HandleStartBattle(User user)
        {
            _eventService.LogEvent(EventType.Highlight, $"User {user.Username} wants to start a battle", null);

            // Start battle if there is already (at least) one other player waiting
            if (_battleQueue.TryDequeue(out var otherOne))
            {
                // TODO: Is there a better way to dequeue a tuple?
                (User otherUser, int otherThread) = otherOne;

                _eventService.LogEvent(EventType.Highlight, $"Opponent found!", null);
                _eventService.LogEvent(EventType.Highlight, $"Starting battle between {user.Username} and {otherUser.Username}!", null);

                string? battleLog = _battleService.StartBattle(user, otherUser);

                // Inform other thread about the result of the battle
                lock (_localLock)
                {
                    _battleResults[otherThread] = battleLog;
                    Monitor.PulseAll(_localLock);
                }

                return (200, battleLog);
            }
            else
            {
                // Wait till another user joins the queue and triggers the start of the battle, then wait to be messaged by the thread handling the 2nd user
                _battleQueue.Enqueue((user, Thread.CurrentThread.ManagedThreadId));

                _eventService.LogEvent(EventType.Highlight, $"There are currently no other players who wish to battle. Adding player to queue...", null);

                lock (_localLock)
                {
                    while (!_battleResults.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                    {
                        Monitor.Wait(_localLock);
                    }

                    string? battleLog = _battleResults[Thread.CurrentThread.ManagedThreadId];

                    // Remove key from dictionary (so that it's clean again if this thread immediately serves another client)
                    // https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2.remove?view=net-9.0
                    _battleResults.Remove(Thread.CurrentThread.ManagedThreadId, out _); // For some reason, the function doesn't work without the second parameter

                    return (200, battleLog);
                }
            }
        }
    }
}

