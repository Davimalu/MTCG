using MTCG.Interfaces;
using MTCG.Interfaces.Logic;
using MTCG.Models;
using MTCG.Models.Enums;

namespace MTCG.Logic
{
    // Has to be moved inside the namespace for some reason
    using BCrypt.Net;

    public class AuthService : IAuthService
    {
        #region Singleton
        private static AuthService? _instance;

        public static AuthService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AuthService();
                }

                return _instance;
            }
        }
        #endregion
        #region DependecyInjection
        public AuthService(IUserService userService, IEventService eventService)
        {
            _userService = userService;
            _eventService = eventService;
        }
        #endregion

        public AuthService() { }

        private readonly IUserService _userService = UserService.Instance;
        private readonly IEventService _eventService = new EventService();

        public bool RegisterUser(string username, string password)
        {
            // Thread Safety: Ensure that the user is not created by a different thread between checking if the user exists and the actual creation of that user
            lock (ThreadSync.UserLock)
            {
                // Check if user already exists
                if (_userService.GetUserByName(username) != null)
                {
                    // User already exists
                    _eventService.LogEvent(EventType.Warning, $"Couldn't register user: User already exists", null);
                    return false;
                }

                // Create user

                // Hash password
                string hashedPassword = HashPassword(password);

                // Add user to database
                _userService.SaveUserToDatabase(new User(username, hashedPassword));
            }

            return true;
        }

        public User? LoginUser(string username, string password)
        {
            // Check if user exists
            User? tmpUser = _userService.GetUserByName(username);

            if (tmpUser != null)
            {
                if (VerifyPassword(password, tmpUser.Password))
                {
                    // Generate token for user
                    string token = $"{tmpUser.Username}-mtcgToken";
                    tmpUser.AuthToken = token;

                    // Update authToken in Database
                    _userService.SaveUserToDatabase(tmpUser);

                    return tmpUser;
                }
                else
                {
                    // Wrong credentials
                    _eventService.LogEvent(EventType.Warning, $"Couldn't log in User {tmpUser.Username}: Invalid password", null);
                    return null;
                }
            }

            // User doesn't exist
            _eventService.LogEvent(EventType.Warning, $"Couldn't log in User {username}: User doesn't exist", null);
            return null;
        }

        private string HashPassword(string password)
        {
            return BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Verify(password, hash);
        }
    }
}
