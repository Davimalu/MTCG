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
using MTCG.Models.Enums;

namespace MTCG.Endpoints
{
    public class UsersEndpoint : IHttpEndpoint
    {
        private readonly AuthService _authService = AuthService.Instance;
        private readonly UserService _userService = UserService.Instance;

        public (int, string?) HandleRequest(TcpClient client, HTTPHeader headers, string? body)
        {
            switch (headers.Method)
            {
                // User Registration
                case "POST":
                    return HandleUserRegistration(body);
                // Retrieve user data
                case "GET":
                    return HandleUserRetrieval(headers);
                // Update user data
                case "PUT":
                    return HandleUserUpdate(headers, body);
                default:
                    return (405, JsonSerializer.Serialize("Method Not Allowed"));
            }
        }


        private (int, string?) HandleUserRegistration(string? body)
        {
            if (body == null) // Check for empty body
            {
                return (400, JsonSerializer.Serialize("Empty request body"));
            }

            // Try to get user information from request body
            User? tmpUser = null;
            try
            {
                tmpUser = JsonSerializer.Deserialize<User>(body);
            }
            catch (Exception ex)
            {
                EventService.LogEvent(EventType.Warning, "Account creation failed: Error parsing request body", ex);
            }
            
            if (tmpUser == null) // Check if Username and Password were provided
            {
                return (400, JsonSerializer.Serialize("Invalid request body"));
            }

            // Try registering the user
            if (_authService.Register(tmpUser.Username, tmpUser.Password))
            {
                EventService.LogEvent(EventType.Highlight, $"New user account created: {tmpUser.Username}", null);

                var response = new
                {
                    message = "User Created",
                    User = new
                    {
                        Username = tmpUser.Username,
                        DisplayName = tmpUser.DisplayName,
                        Biography = tmpUser.Biography,
                        Image = tmpUser.Image,
                        Stats = tmpUser.Stats,
                        Stack = tmpUser.Stack,
                        Deck = tmpUser.Deck,
                        CoinCount = tmpUser.CoinCount
                    }
                };

                return (201, JsonSerializer.Serialize(response));
            }
            else
            {
                return (409, JsonSerializer.Serialize("User already exists"));
            }
        }

        private (int, string?) HandleUserRetrieval(HTTPHeader headers)
        {
            // Check if the user which information is to be retrieved is the same user as the authenticated user
            AuthenticationError authorizationError = RequestedUserIsAuthenticatedUser(headers);

            switch (authorizationError)
            {
                case AuthenticationError.InvalidPath:
                    return (400, JsonSerializer.Serialize("Invalid path"));
                case AuthenticationError.NonExistentUser:
                    return (404, JsonSerializer.Serialize("User doesn't exist"));
                case AuthenticationError.UnauthenticatedUser:
                    return (401, JsonSerializer.Serialize("User not authenticated"));
                case AuthenticationError.UnauthorizedUser:
                    return (403, JsonSerializer.Serialize("User not authorized"));
                default:
                    // Retrieve user information | values != null, otherwise one of the other cases would have happened
                    string token = HeaderHelper.GetTokenFromHeader(headers)!;
                    User userByToken = _userService.GetUserByToken(token)!;
                    return (200, _userService.UserToJson(userByToken));
            }
        }

        private (int, string?) HandleUserUpdate(HTTPHeader headers, string? body)
        {
            // Check if the user which information is to be updated is the same user as the authenticated user
            AuthenticationError authorizationError = RequestedUserIsAuthenticatedUser(headers);

            User userToBeUpdated;
            switch (authorizationError)
            {
                case AuthenticationError.InvalidPath:
                    return (400, JsonSerializer.Serialize("Invalid path"));
                case AuthenticationError.NonExistentUser:
                    return (404, JsonSerializer.Serialize("User doesn't exist"));
                case AuthenticationError.UnauthenticatedUser:
                    return (401, JsonSerializer.Serialize("User not authenticated"));
                case AuthenticationError.UnauthorizedUser:
                    return (403, JsonSerializer.Serialize("User not authorized"));
                default:
                    // Retrieve user information | values != null, otherwise one of the other cases would have happened
                    string token = HeaderHelper.GetTokenFromHeader(headers)!;
                    userToBeUpdated = _userService.GetUserByToken(token)!;
                    break;
            }

            // Update user information
            if (body == null)
            {
                EventService.LogEvent(EventType.Warning, $"Couldn't update user information of User {userToBeUpdated.Username}: No body provided", null);
                return (400, JsonSerializer.Serialize("No information provided"));
            }

            // Deserialize request body into dictionary
            Dictionary<string, string>? updatedInformation;
            try
            {
                updatedInformation = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
            }
            catch (Exception ex)
            {
                EventService.LogEvent(EventType.Warning, $"Couldn't update user information of User {userToBeUpdated.Username}: Invalid request body", ex);
                return (400, JsonSerializer.Serialize("Invalid Request Body"));
            } 

            if (updatedInformation == null)
            {
                EventService.LogEvent(EventType.Warning, $"Couldn't update user information of User {userToBeUpdated.Username}: Invalid request body", null);
                return (400, JsonSerializer.Serialize("Invalid Request Body"));
            }

            // https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1.getvalueordefault?view=net-9.0#system-nullable-1-getvalueordefault
            userToBeUpdated.DisplayName = updatedInformation.GetValueOrDefault("Name", userToBeUpdated.DisplayName);
            userToBeUpdated.Biography = updatedInformation.GetValueOrDefault("Bio", userToBeUpdated.Biography);
            userToBeUpdated.Image = updatedInformation.GetValueOrDefault("Image", userToBeUpdated.Image);

            // Update user
            _userService.SaveUserToDatabase(userToBeUpdated);

            return (200, JsonSerializer.Serialize("User information updated"));
        }

        /// <summary>
        /// checks whether the user in the HTTP Path (e.g. /users/kienboec) is the same as the authenticated user (authenticated by token, e.g. kienboec-mtcgToken)
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        private AuthenticationError RequestedUserIsAuthenticatedUser(HTTPHeader headers)
        {
            // Check if path is valid
            if (!IsValidRetrievalPath(headers.Path))
            {
                return AuthenticationError.InvalidPath;
            }

            // Get username from path
            string username = headers.Path.Split('/').Last();
            User? userByName = _userService.GetUserByName(username);

            if (userByName == null)
            {
                return AuthenticationError.NonExistentUser;
            }

            // Get username from authentication token
            string token = HeaderHelper.GetTokenFromHeader(headers)!;
            User? userByToken = _userService.GetUserByToken(token);

            if (userByToken == null)
            {
                return AuthenticationError.UnauthenticatedUser;
            }

            // Username from path (user to be updated or retrieved) doesn't match username from authentication token
            if (userByName.Username != userByToken.Username)
            {
                EventService.LogEvent(EventType.Warning, $"User {userByToken.Username} tried to illegitimately edit/access information of User {userByName.Username}", null);
                return AuthenticationError.UnauthorizedUser;
            }

            return AuthenticationError.NoError;
        }

        /// <summary>
        /// checks if the provided path is in format "/users/username"
        /// </summary>
        /// <param name="path"></param>
        /// <returns>
        /// <para>true if the path matches the format</para>
        /// <para>false if the path doesn't match the format</para>
        /// </returns>
        private bool IsValidRetrievalPath(string path)
        {
            // Regex pattern
            string pattern = @"^/users/[a-zA-Z0-9_-]+$";
            return Regex.IsMatch(path, pattern);
        }
    }
}
