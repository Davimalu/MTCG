using MTCG.HTTP;
using MTCG.Interfaces;
using MTCG.Interfaces.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Enums;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MTCG.Endpoints
{
    public class UsersEndpoint : IHttpEndpoint
    {
        private readonly IAuthService _authService = AuthService.Instance;
        private readonly IUserService _userService = UserService.Instance;
        private readonly IEventService _eventService = new EventService();
        private readonly IHttpHeaderService _ihttpHeaderService = new HttpHeaderService();

        public UsersEndpoint()
        {

        }

        #region DependencyInjection
        // Unit Testing
        public UsersEndpoint(IAuthService authService, IEventService eventService, IUserService userService, IHttpHeaderService ihttpHeaderService)
        {
            _authService = authService;
            _eventService = eventService;
            _userService = userService;
            _ihttpHeaderService = ihttpHeaderService;
        }
        #endregion

        public (int, string?) HandleRequest(TcpClient? client, HttpHeader headers, string? body)
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
                _eventService.LogEvent(EventType.Warning, "Account creation failed: Error parsing request body", ex);
            }

            if (tmpUser == null) // Check if Username and Password were provided
            {
                return (400, JsonSerializer.Serialize("Invalid request body"));
            }

            // Thread Safety: Ensure that two threads don't try to register the same user at the same time
            lock (ThreadSync.UserLock)
            {
                // Try registering the user
                if (_authService.RegisterUser(tmpUser.Username, tmpUser.Password))
                {
                    _eventService.LogEvent(EventType.Highlight, $"New user account created: {tmpUser.Username}", null);

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
        }


        private (int, string?) HandleUserRetrieval(HttpHeader headers)
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
                    string token = _ihttpHeaderService.GetTokenFromHeader(headers)!;
                    User userByToken = _userService.GetUserByToken(token)!;
                    return (200, _userService.UserToJson(userByToken));
            }
        }

        private (int, string?) HandleUserUpdate(HttpHeader headers, string? body)
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
                    string token = _ihttpHeaderService.GetTokenFromHeader(headers)!;
                    userToBeUpdated = _userService.GetUserByToken(token)!;
                    break;
            }

            // Update user information
            if (body == null)
            {
                _eventService.LogEvent(EventType.Warning, $"Couldn't update user information of User {userToBeUpdated.Username}: No body provided", null);
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
                _eventService.LogEvent(EventType.Warning, $"Couldn't update user information of User {userToBeUpdated.Username}: Invalid request body", ex);
                return (400, JsonSerializer.Serialize("Invalid Request Body"));
            }

            if (updatedInformation == null)
            {
                _eventService.LogEvent(EventType.Warning, $"Couldn't update user information of User {userToBeUpdated.Username}: Invalid request body", null);
                return (400, JsonSerializer.Serialize("Invalid Request Body"));
            }

            // https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1.getvalueordefault?view=net-9.0#system-nullable-1-getvalueordefault
            userToBeUpdated.DisplayName = updatedInformation.GetValueOrDefault("Name", userToBeUpdated.DisplayName);
            userToBeUpdated.Biography = updatedInformation.GetValueOrDefault("Bio", userToBeUpdated.Biography);
            userToBeUpdated.Image = updatedInformation.GetValueOrDefault("Image", userToBeUpdated.Image);

            // Update user
            _userService.SaveUserToDatabase(userToBeUpdated);

            var response = new
            {
                message = "User information updated",
                User = new
                {
                    Username = userToBeUpdated.Username,
                    DisplayName = userToBeUpdated.DisplayName,
                    Biography = userToBeUpdated.Biography,
                    Image = userToBeUpdated.Image,
                    Stats = userToBeUpdated.Stats,
                    CoinCount = userToBeUpdated.CoinCount
                }
            };

            return (200, JsonSerializer.Serialize(response));
        }

        /// <summary>
        /// checks whether the user in the HTTP Path (e.g. /users/kienboec) is the same as the authenticated user (authenticated by token, e.g. kienboec-mtcgToken)
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        public AuthenticationError RequestedUserIsAuthenticatedUser(HttpHeader headers)
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
            string token = _ihttpHeaderService.GetTokenFromHeader(headers)!;
            User? userByToken = _userService.GetUserByToken(token);

            if (userByToken == null)
            {
                return AuthenticationError.UnauthenticatedUser;
            }

            // Username from path (user to be updated or retrieved) doesn't match username from authentication token
            if (userByName.Username != userByToken.Username)
            {
                _eventService.LogEvent(EventType.Warning, $"User {userByToken.Username} tried to illegitimately edit/access information of User {userByName.Username}", null);
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
