using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MTCG.HTTP;
using MTCG.Interfaces;
using MTCG.Logic;
using MTCG.Models;

namespace MTCG.Endpoints
{
    public class UsersEndpoint : IHttpEndpoint
    {
        private readonly AuthService _authService = AuthService.Instance;
        private readonly UserService _userService = UserService.Instance;

        public (int, string?) HandleRequest(TcpClient client, HTTPHeader headers, string? body)
        {
            // User Registration
            if (headers.Method == "POST")
            {
                User? tempUser = JsonSerializer.Deserialize<User>(body);

                // Check if Username and Password were provided
                if (tempUser == null)
                {
                    return (400, JsonSerializer.Serialize("Invalid request body"));
                }

                // Try registering the user
                if (_authService.Register(tempUser.Username, tempUser.Password))
                {
                    return (201, JsonSerializer.Serialize("User Created"));
                }
                else
                {
                    return (409, JsonSerializer.Serialize("User already exists"));
                }
            }
            
            // Retrieve user data
            if (headers.Method == "GET")
            {
                // Check if path is valid
                if (!IsValidPath(headers.Path))
                {
                    return (400, JsonSerializer.Serialize("Invalid path"));
                }

                // Get username from path
                string username = headers.Path.Split('/').Last();
                User? userByName = _userService.GetUserByName(username);

                if (userByName == null)
                {
                    return (404, JsonSerializer.Serialize("User doesn't exist"));
                }

                // Check if user is authorized
                string token = HeaderHelper.GetTokenFromHeader(headers)!;
                User? userByToken = _userService.GetUserByToken(token);

                if (userByToken == null)
                {
                    return (401, JsonSerializer.Serialize("User not authorized"));
                }

                // Requested username doesn't match authorized user
                if (userByName.Username != userByToken.Username)
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine($"[WARNING] User {userByToken.Username} tried to illegitimately access information of User {userByName.Username}");
                    Console.ResetColor();

                    return (401, JsonSerializer.Serialize("User not authorized"));
                }

                return (200, _userService.UserToJson(userByName));
            }

            // Update user data
            if (headers.Method == "PUT")
            {
                // Check if path is valid
                if (!IsValidPath(headers.Path))
                {
                    return (400, JsonSerializer.Serialize("Invalid path"));
                }

                // TODO: Code is very similar to GET Method -> REFACTOR

                // Get username from path
                string username = headers.Path.Split('/').Last();
                User? userByName = _userService.GetUserByName(username);

                if (userByName == null)
                {
                    return (404, JsonSerializer.Serialize("User doesn't exist"));
                }

                // Check if user is authorized
                string token = HeaderHelper.GetTokenFromHeader(headers)!;
                User? userByToken = _userService.GetUserByToken(token);

                if (userByToken == null)
                {
                    return (401, JsonSerializer.Serialize("User not authorized"));
                }

                // Provided username doesn't match authorized user
                if (userByName.Username != userByToken.Username)
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine($"[WARNING] User {userByToken.Username} tried to illegitimately edit information of User {userByName.Username}");
                    Console.ResetColor();

                    return (401, JsonSerializer.Serialize("User not authorized"));
                }

                // Update user information
                if (body == null)
                {
                    return (400, JsonSerializer.Serialize("No information provided"));
                }

                // Deserialize request body into dictionary
                var updatedInformation = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

                if (updatedInformation == null)
                {
                    return (400, JsonSerializer.Serialize("Invalid Request Body"));
                }

                if (updatedInformation.TryGetValue("Name", out string? chosenName))
                {
                    userByName.DisplayName = chosenName;
                }

                if (updatedInformation.TryGetValue("Bio", out string? biography))
                {
                    userByName.Biography = biography;
                }

                if (updatedInformation.TryGetValue("Image", out string? image))
                {
                    userByName.Image = image;
                }

                // Update user
                _userService.SaveUserToDatabase(userByName);

                return (200, JsonSerializer.Serialize("User information updated"));
            }

            return (405, "Method Not Allowed");
        }

        /// <summary>
        /// checks if the provided path is in format "/users/username"
        /// </summary>
        /// <param name="path"></param>
        /// <returns>
        /// <para>true if the path matches the format</para>
        /// <para>false if the path doesn't match the format</para>
        /// </returns>
        private bool IsValidPath(string path)
        {
            // Regex pattern
            string pattern = @"^/users/[a-zA-Z0-9_-]+$";
            return Regex.IsMatch(path, pattern);
        }
    }
}
