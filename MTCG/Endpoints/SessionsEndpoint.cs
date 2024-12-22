using MTCG.Logic;
using MTCG.Models;
using System.Net.Sockets;
using System.Text.Json;
using MTCG.Models.Enums;
using MTCG.Interfaces.Logic;
using MTCG.Interfaces.HTTP;

namespace MTCG.Endpoints
{
    public class SessionsEndpoint : IHttpEndpoint
    {
        private readonly IAuthService _authService = AuthService.Instance;
        private readonly IEventService _eventService = new EventService();

        public SessionsEndpoint()
        {

        }

        #region DependencyInjection
        public SessionsEndpoint(IAuthService authService, IEventService eventService)
        {
            _authService = authService;
            _eventService = eventService;
        }
        #endregion

        public (int, string?) HandleRequest(TcpClient? client, HttpHeader headers, string? body)
        {
            switch (headers.Method)
            {
                case "POST":
                    return HandleUserLogin(body);
                default:
                    return (405, JsonSerializer.Serialize("Method Not Allowed"));
            }
        }

        private (int, string?) HandleUserLogin(string? body)
        {
            // Check for valid input
            if (body == null)
            {
                return (400, JsonSerializer.Serialize("Empty request body"));
            }

            User? tmpUser = null;
            try
            {
                tmpUser = JsonSerializer.Deserialize<User>(body);
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Warning, $"Couldn't parse request body provided by user trying to log in" ,ex);
                return (400, JsonSerializer.Serialize("Invalid request body"));
            }

            if (tmpUser == null)
            {
                return (400, JsonSerializer.Serialize("Invalid request body"));
            }

            // If none of these checks failed, try to log the user in
            User? user = _authService.LoginUser(tmpUser.Username, tmpUser.Password);

            if (user == null)
            {
                return (401, JsonSerializer.Serialize("Wrong username or password"));
            }
            else
            {
                var response = new
                {
                    message = "Login successful",
                    Token = user.AuthToken
                };

                string jsonString = JsonSerializer.Serialize(response);

                _eventService.LogEvent(EventType.Highlight, $"User {user.Username} logged in successfully", null);
                return (200, jsonString);
            }
        }
    }
}
