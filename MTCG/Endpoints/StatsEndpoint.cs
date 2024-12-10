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
    public class StatsEndpoint : IHttpEndpoint
    {
        private readonly UserService _userService = UserService.Instance;

        public (int, string?) HandleRequest(TcpClient client, HTTPHeader headers, string? body)
        {
            // Get stats of user
            if (headers.Method == "GET")
            {
                // Check if user is authorized
                string token = HeaderHelper.GetTokenFromHeader(headers)!;
                User? user = _userService.GetUserByToken(token);

                Thread.Sleep(5000);

                if (user == null)
                {
                    return (401, JsonSerializer.Serialize("User not authorized"));
                }

                string json = JsonSerializer.Serialize(user.Stats);
                return (200, json);
            }

            return (405, "Method not allowed");
        }
    }
}
