using MTCG.Endpoints;
using MTCG.Interfaces.Logic;
using MTCG.Models;
using MTCG.Models.Enums;
using NSubstitute;
using System.Text.Json;

namespace MTCGTests.Endpoints
{
    public class TestSessionsEndpoint
    {
        private IAuthService _authService;
        private IEventService _eventService;
        private SessionsEndpoint _endpoint;

        [SetUp]
        public void SetUp()
        {
            _authService = Substitute.For<IAuthService>();
            _eventService = Substitute.For<IEventService>();

            _endpoint = new SessionsEndpoint(_authService, _eventService);
        }

        [Test]
        public void HandleRequest_POST_EmptyBody_Returns400WithErrorMessage()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/sessions", Method = "POST", Version = "1.1" };

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(400));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Empty request body")));
        }

        [Test]
        public void HandleRequest_POST_InvalidJson_Returns400WithErrorMessage()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/sessions", Method = "POST", Version = "1.1" };
            var invalidBody = "Invalid JSON";

            // Act
            var result = _endpoint.HandleRequest(null, headers, invalidBody);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(400));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Invalid request body")));
            _eventService.Received(1).LogEvent(EventType.Warning, Arg.Any<string>(), Arg.Any<Exception>());
        }

        [Test]
        public void HandleRequest_POST_InvalidCredentials_Returns401WithErrorMessage()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/sessions", Method = "POST", Version = "1.1" };
            var body = JsonSerializer.Serialize(new User { Username = "test", Password = "wrongPassword" });
            _authService.Login("test", "wrongPassword").Returns((User?)null);

            // Act
            var result = _endpoint.HandleRequest(null, headers, body);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(401));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Wrong username or password")));
            _eventService.Received(1).LogEvent(EventType.Warning, Arg.Any<string>(), null);
        }

        [Test]
        public void HandleRequest_POST_ValidCredentials_Returns200WithSuccessMessage()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/sessions", Method = "POST", Version = "1.1" };
            var body = JsonSerializer.Serialize(new User { Username = "test", Password = "correctPassword" });
            var user = new User { Username = "test", AuthToken = "valid-token" };
            _authService.Login("test", "correctPassword").Returns(user);

            // Act
            var result = _endpoint.HandleRequest(null, headers, body);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(200));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize(new { message = "Login successful", Token = "valid-token" })));
            _eventService.Received(1).LogEvent(EventType.Highlight, Arg.Any<string>(), null);
        }

        [Test]
        public void HandleRequest_GET_UnsupportedMethod_Returns405WithErrorMessage()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/sessions", Method = "GET", Version = "1.1" };

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(405));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Method Not Allowed")));
        }
    }
}
