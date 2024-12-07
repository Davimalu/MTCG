using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using MTCG.Interfaces;
using MTCG.Logic;
using MTCG.Models;

namespace MTCG.Endpoints
{
    public class SessionsEndpoint : IHttpEndpoint
    {
        private readonly AuthService _authService = AuthService.Instance;

        public (int, string?) HandleRequest(TcpClient client, HTTPHeader headers, string? body)
        {
            // User Login
            if (headers.Method == "POST")
            {
                User? tempUser = JsonSerializer.Deserialize<User>(body);

                // Check if Username and Password were provided
                if (tempUser == null)
                {
                    return (400, JsonSerializer.Serialize("Invalid request body"));
                }

                User? user = _authService.Login(tempUser.Username, tempUser.Password);

                if (user == null)
                {
                    return (401, JsonSerializer.Serialize("Wrong username or password"));
                }
                else
                {
                    var jsonObject = new Dictionary<string, string>
                    {
                        { "Token", user.AuthToken }
                    };

                    string jsonString = JsonSerializer.Serialize(jsonObject);

                    return (200, jsonString);
                }
            }

            return (405, JsonSerializer.Serialize("Method Not Allowed"));
        }
    }
}
