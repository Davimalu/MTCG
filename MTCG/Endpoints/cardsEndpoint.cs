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
    public class CardsEndpoint : IHttpEndpoint
    {
        private readonly UserService _userService = UserService.Instance;

        public (int, string?) HandleRequest(TcpClient client, HTTPHeader headers, string? body)
        {
            // Get list of cards
            if (headers.Method == "GET")
            {
                // Check if user is authorized
                string? token = HeaderHelper.GetTokenFromHeader(headers)!;
                User? user = _userService.GetUserByToken(token);

                if (user == null)
                {
                    return (401, JsonSerializer.Serialize("User not authorized"));
                }

                // Convert user stack into JSON
                string json = JsonSerializer.Serialize(user.Stack);

                return (200, json);
            }

            return (405, JsonSerializer.Serialize("Method not allowed"));
        }
    }
}
