using System;
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
    public class ScoreboardEndpoint : IHttpEndpoint
    {
        private readonly UserService _userService = UserService.Instance;
        private readonly ScoreboardService _scoreboardService = ScoreboardService.Instance;

        public (int, string?) HandleRequest(TcpClient client, HTTPHeader headers, string? body)
        {
            // Check if user is authorized
            string token = HeaderHelper.GetTokenFromHeader(headers)!;
            User? user = _userService.GetUserByToken(token);

            if (user == null)
            {
                return (401, JsonSerializer.Serialize("User not authorized"));
            }

            // Get scoreboard
            if (headers.Method == "GET")
            {
                Scoreboard scoreboard = new Scoreboard();
                _scoreboardService.FillScoreboard(scoreboard);

                string json = JsonSerializer.Serialize(scoreboard.Entries);
                return (200, json);
            }

            return (405, "Method Not Allowed");
        }
    }
}
