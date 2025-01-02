using MTCG.Interfaces.Logic;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Enums;
using NSubstitute;

namespace MTCGTests.Logic
{
    public class TestAuthService
    {
        private AuthService _authService;
        private IUserService _userService;
        private IEventService _eventService;

        [SetUp]
        public void SetUp()
        {
            _userService = Substitute.For<IUserService>();
            _eventService = Substitute.For<IEventService>();

            _authService = new AuthService(_userService, _eventService);
        }

        [Test]
        public void RegisterUser_UserAlreadyExists_ReturnsFalse()
        {
            // Arrange
            string username = "existingUser";
            string password = "password123";
            _userService.GetUserByName(username).Returns(new User(username, "hashedPassword"));

            // Act
            bool result = _authService.RegisterUser(username, password);

            // Assert
            Assert.That(result, Is.False);
            _eventService.Received(1).LogEvent(EventType.Warning, Arg.Is<string>(msg => msg.Contains("User already exists")), null);
        }

        [Test]
        public void RegisterUser_NewUser_ReturnsTrue()
        {
            // Arrange
            string username = "newUser";
            string password = "password123";
            _userService.GetUserByName(username).Returns((User?)null);

            // Act
            bool result = _authService.RegisterUser(username, password);

            // Assert
            Assert.That(result, Is.True);
            _userService.Received(1).SaveUserToDatabase(Arg.Is<User>(u => u.Username == username));
        }

        [Test]
        public void LoginUser_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            string username = "nonExistentUser";
            string password = "password123";
            _userService.GetUserByName(username).Returns((User?)null);

            // Act
            User? result = _authService.LoginUser(username, password);

            // Assert
            Assert.That(result, Is.Null);
            _eventService.Received(1).LogEvent(EventType.Warning, Arg.Is<string>(msg => msg.Contains("User doesn't exist")), null);
        }

        [Test]
        public void LoginUser_InvalidPassword_ReturnsNull()
        {
            // Arrange
            string username = "validUser";
            string password = "wrongPassword";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctPassword");

            _userService.GetUserByName(username).Returns(new User(username, hashedPassword));

            // Act
            User? result = _authService.LoginUser(username, password);

            // Assert
            Assert.That(result, Is.Null);
            _eventService.Received(1).LogEvent(EventType.Warning, Arg.Is<string>(msg => msg.Contains("Invalid password")), null);
        }

        [Test]
        public void LoginUser_ValidCredentials_ReturnsUser()
        {
            // Arrange
            string username = "validUser";
            string password = "correctPassword";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            User user = new User(username, hashedPassword);
            _userService.GetUserByName(username).Returns(user);

            // Act
            User? result = _authService.LoginUser(username, password);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.AuthToken, Is.EqualTo("validUser-mtcgToken"));
            _userService.Received(1).SaveUserToDatabase(Arg.Is<User>(u => u.AuthToken == "validUser-mtcgToken"));
        }
    }
}
