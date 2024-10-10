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
    public class UsersEndpoint : IHttpEndpoint
    {
        public (int, string?) HandleRequest(string method, string body, AuthService AuthService)
        {
            // User Registration
            if (method == "POST")
            {
                User? tempUser = JsonSerializer.Deserialize<User>(body);

                // Check if Username and Password were provided
                if (tempUser == null)
                {
                    return (400, "Invalid data provided");
                }

                // Try registering the user
                if (AuthService.Register(tempUser.Username, tempUser.Password))
                {
                    return (201, "User Created");
                }
                else
                {
                    return (409, "User already exists");
                }
            }

            return (400, "Bad Request");
        }
    }
}
