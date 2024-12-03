using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private ConcurrentQueue<User> _battleQueue = new ConcurrentQueue<User>();
        private readonly UserService _userService = UserService.Instance;
        private readonly BattleService _battleService = new BattleService();

        public (int, string?) HandleRequest(HTTPHeader headers, string? body)
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
                _battleQueue.Enqueue(user);

                // Start battle if there is already (at least) one other player waiting
                if (_battleQueue.Count >= 2)
                {
                    _battleQueue.TryDequeue(out User playerA);
                    _battleQueue.TryDequeue(out User playerB);

                    User? winner = _battleService.StartBattle(playerA, playerB);
                }
                else
                {
                    // Wait till another user joins the queue and triggers the start of the battle, fetch the result
                    return (501, "Not implemented");
                }
            }

            return (405, "Method Not Allowed");
        }
    }
}

