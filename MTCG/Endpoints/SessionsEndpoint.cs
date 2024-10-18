using System;
using System.Collections.Generic;
using System.Linq;
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
        public (int, string?) HandleRequest(string method, string body, AuthService AuthService)
        {
            // User Login
            if (method == "POST")
            {
                User? tempUser = JsonSerializer.Deserialize<User>(body);

                // Check if Username and Password were provided
                if (tempUser == null)
                {
                    return (400, "Invalid data provided");
                }

                User? user = AuthService.Login(tempUser.Username, tempUser.Password);

                if (user == null)
                {
                    return (401, "Login failed");
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

            return (400, "Invalid Request");
        }
    }
}
