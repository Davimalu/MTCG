using System;
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
    public class CardsEndpoint : IHttpEndpoint
    {
        private readonly UserService _userService = UserService.Instance;

        public (int, string?) HandleRequest(HTTPHeader headers, string body)
        {
            // Get list of cards
            if (headers.Method == "GET")
            {
                string token = HeaderHelper.GetTokenFromHeader(headers)!;
                User? user = _userService.GetUserByToken(token);

                // If the user doesn't exist, NULL is returned
                if (user == null)
                {
                    return (403, "User not authorized!");
                }

                // Convert user stack into JSON
                string json = JsonSerializer.Serialize(user.Stack);

                return (200, json);
            }

            return (405, "Method Not Allowed");
        }
    }
}
