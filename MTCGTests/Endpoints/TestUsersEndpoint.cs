using MTCG.Endpoints;
using MTCG.HTTP;
using MTCG.Interfaces;
using MTCG.Models;
using MTCG.Models.Enums;
using NSubstitute;
using System.Text.Json;

namespace MTCGTests.Endpoints
{
    public class TestUsersEndpoint
    {
        private IAuthService _authService;
        private IEventService _eventService;
        private IUserService _userService;
        private IHeaderHelper _headerHelper;
        private UsersEndpoint _usersEndpoint;

        [SetUp]
        public void Setup()
        {
            _authService = Substitute.For<IAuthService>();
            _eventService = Substitute.For<IEventService>();
            _userService = Substitute.For<IUserService>();
            _headerHelper = Substitute.For<IHeaderHelper>();

            _usersEndpoint = new UsersEndpoint(_authService, _eventService, _userService, _headerHelper);

        }

        [Test]
        public void HandleRequest_POST_EmptyBodyReturns400()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/users", Method = "POST", Version = "1.1" };

            // Act
            var result = _usersEndpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(400));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Empty request body")));
        }

        [Test]
        public void HandleRequest_POST_InvalidJsonReturns400()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/users", Method = "POST", Version = "1.1" };
            string invalidJson = "{ Invalid [JS]ON";

            // Act
            var result = _usersEndpoint.HandleRequest(null, headers, invalidJson);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(400));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Invalid request body")));
            _eventService.Received().LogEvent(EventType.Warning, Arg.Any<string>(), Arg.Any<Exception>());
        }

        [Test]
        public void HandleRequest_POST_ValidUserRegistrationReturns201()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/users", Method = "POST", Version = "1.1" };
            var body = JsonSerializer.Serialize(new User { Username = "testuser", Password = "password" });

            _authService.Register("testuser", "password").Returns(true);

            // Act
            var result = _usersEndpoint.HandleRequest(null, headers, body);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(201));
            Assert.That(result.Item2, Does.Contain("User Created"));
            _eventService.Received().LogEvent(EventType.Highlight, Arg.Any<string>(), null);
        }

        [Test]
        public void HandleRequest_POST_AlreadyExistingUserRegistrationReturns409()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/users", Method = "POST", Version = "1.1" };
            var body = JsonSerializer.Serialize(new User { Username = "testuser", Password = "password" });

            _authService.Register("testuser", "password").Returns(false);

            // Act
            var result = _usersEndpoint.HandleRequest(null, headers, body);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(409));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("User already exists")));
        }

        [Test]
        public void HandleRequest_GET_InvalidPathReturns400()
        {
            // Arrange
            var headers = new HTTPHeader { Method = "GET", Path = "/invalid/path", Version = "1.1" };

            // Act
            var result = _usersEndpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(400));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Invalid path")));
        }

        [Test]
        public void HandleRequest_GET_NonExistentUserReturns404()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/users/nonexistent", Method = "GET", Version = "1.1" };
            _userService.GetUserByName("nonexistent").Returns((User?)null);

            // Act
            var result = _usersEndpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(404));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("User doesn't exist")));
        }

        [Test]
        public void HandleRequest_GET_ValidPathReturns200AndUserJson()
        {
            // Arrange
            var headers = new HTTPHeader { Method = "GET", Path = "/users/testuser", Version = "1.1" };
            var token = "testuser-mtcgToken";
            var user = new User { Username = "testuser" };

            _headerHelper.GetTokenFromHeader(headers).Returns(token);
            _userService.GetUserByName("testuser").Returns(user);
            _userService.GetUserByToken(token).Returns(user);
            _userService.UserToJson(user).Returns(JsonSerializer.Serialize(user));

            // Act
            var result = _usersEndpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(200));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize(user)));
        }

        [Test]
        public void HandleRequest_GET_UnauthenticatedUserReturns401()
        {
            // Arrange
            var headers = new HTTPHeader { Method = "GET", Path = "/users/testuser", Version = "1.1" };
            var token = "invalid-mtcgToken";
            var user = new User { Username = "testuser" };

            _headerHelper.GetTokenFromHeader(headers).Returns(token);
            _userService.GetUserByName("testuser").Returns(user);
            _userService.GetUserByToken(token).Returns((User?)null);

            // Act
            var result = _usersEndpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(401));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("User not authenticated")));
        }

        [Test]
        public void HandleRequest_PUT_NoBodyReturns400AndLogsWarning()
        {
            // Arrange
            var headers = new HTTPHeader { Method = "PUT", Path = "/users/testuser", Version = "1.1" };
            var token = "testuser-mtcgToken";
            var user = new User { Username = "testuser" };

            _headerHelper.GetTokenFromHeader(headers).Returns(token);
            _userService.GetUserByName("testuser").Returns(user);
            _userService.GetUserByToken(token).Returns(user);

            // Act
            var result = _usersEndpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(400));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("No information provided")));
            _eventService.Received().LogEvent(EventType.Warning, Arg.Any<string>(), null);
        }

        [Test]
        public void HandleRequest_PUT_InvalidBodyReturns400AndLogsWarning()
        {
            // Arrange
            var headers = new HTTPHeader { Method = "PUT", Path = "/users/testuser", Version = "1.1" };
            var token = "testuser-mtcgToken";
            var user = new User { Username = "testuser" };

            _headerHelper.GetTokenFromHeader(headers).Returns(token);
            _userService.GetUserByName("testuser").Returns(user);
            _userService.GetUserByToken(token).Returns(user);

            var body = "{ Invalid [JS]ON";

            // Act
            var result = _usersEndpoint.HandleRequest(null, headers, body);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(400));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Invalid Request Body")));
            _eventService.Received().LogEvent(EventType.Warning, Arg.Any<string>(), Arg.Any<Exception>());
        }

        [Test]
        public void HandleRequest_PUT_ValidBodyUpdatesUserAndReturns200()
        {
            // Arrange
            var headers = new HTTPHeader { Method = "PUT", Path = "/users/testuser", Version = "1.1" };
            var token = "testuser-mtcgToken";
            var user = new User { Username = "testuser" };

            _headerHelper.GetTokenFromHeader(headers).Returns(token);
            _userService.GetUserByName("testuser").Returns(user);
            _userService.GetUserByToken(token).Returns(user);

            var body = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                { "Name", "Updated Name" },
                { "Bio", "Updated Bio" },
                { "Image", "Updated Image" }
            });

            // Act
            var result = _usersEndpoint.HandleRequest(null, headers, body);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(200));

            var expectedResponse = new
            {
                message = "User information updated",
                User = new
                {
                    Username = "testuser",
                    DisplayName = "Updated Name",
                    Biography = "Updated Bio",
                    Image = "Updated Image",
                    Stats = user.Stats,
                    CoinCount = user.CoinCount
                }
            };
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize(expectedResponse)));

            _userService.Received().SaveUserToDatabase(Arg.Is<User>(u =>
                u.DisplayName == "Updated Name" &&
                u.Biography == "Updated Bio" &&
                u.Image == "Updated Image"));
        }


        [Test]
        public void HandleRequest_InvalidMethod_Returns405()
        {
            var headers = new HTTPHeader { Path = "/users", Method = "DELETE", Version = "1.1" };

            var result = _usersEndpoint.HandleRequest(null, headers, null);

            Assert.That(result.Item1, Is.EqualTo(405));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Method Not Allowed")));
        }
    }
}
