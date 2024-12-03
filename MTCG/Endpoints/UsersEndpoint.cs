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
        private readonly AuthService _authService = AuthService.Instance;
        private readonly UserService _userService = UserService.Instance;

        public (int, string?) HandleRequest(HTTPHeader headers, string? body)
        {
            // User Registration
            if (headers.Method == "POST")
            {
                User? tempUser = JsonSerializer.Deserialize<User>(body);

                // Check if Username and Password were provided
                if (tempUser == null)
                {
                    return (400, "Invalid data provided");
                }

                // Try registering the user
                if (_authService.Register(tempUser.Username, tempUser.Password))
                {
                    return (201, "User Created");
                }
                else
                {
                    return (409, "User already exists");
                }
            }
            
            // Retrieve user data
            if (headers.Method == "GET")
            {
                // TODO: WIP

                // Get username from path
                string username = headers.Path.Split('/').Last();

                User? tmpUser = _userService.GetUserByName(username);
            }

            return (400, "Bad Request");
        }
    }
}
