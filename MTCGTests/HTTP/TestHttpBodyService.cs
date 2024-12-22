using MTCG.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Models;
using MTCG.Models.Enums;
using NSubstitute;

namespace MTCGTests.HTTP
{
    public class TestHttpBodyService
    {
        private HttpBodyService _httpBodyService;
        private IEventService _eventService;

        [SetUp]
        public void Setup()
        {
            _eventService = Substitute.For<IEventService>();
            _httpBodyService = new HttpBodyService(_eventService);
        }

        [Test]
        public void ParseHTTPBody_ShouldReturnBody_WhenContentLengthIsPresent()
        {
            // Arrange
            var headers = new HttpHeader
            {
                Method = "POST",
                Path = "/users",
                Version = "HTTP/1.1"
            };
            headers.Headers["Content-Length"] = "5";

            using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Hello"));
            using var reader = new StreamReader(memoryStream);

            // Act
            var result = _httpBodyService.ParseHttpBody(reader, headers);

            // Assert
            Assert.That(result, Is.EqualTo("Hello"));
        }

        [Test]
        public void ParseHTTPBody_ShouldReturnNull_WhenContentLengthIsMissing()
        {
            // Arrange
            var headers = new HttpHeader
            {
                Method = "GET",
                Path = "/users",
                Version = "HTTP/1.1"
            }; // No "Content-Length" key

            using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Hello"));
            using var reader = new StreamReader(memoryStream);

            // Act
            var result = _httpBodyService.ParseHttpBody(reader, headers);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void ParseHTTPBody_ShouldLogWarningAndReturnNull_WhenContentLengthIsInvalid()
        {
            // Arrange
            var headers = new HttpHeader
            {
                Method = "PUT",
                Path = "/users",
                Version = "HTTP/1.1"
            };
            headers.Headers["Content-Length"] = "invalid";

            using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Hello"));
            using var reader = new StreamReader(memoryStream);

            // Act
            var result = _httpBodyService.ParseHttpBody(reader, headers);

            // Assert
            Assert.That(result, Is.Null);
            _eventService.Received(1).LogEvent(EventType.Warning,
                "Couldn't parse HTTP Request Body: Invalid Content Length",
                Arg.Any<Exception>());
        }

        [Test]
        public void ParseHTTPBody_ShouldLogWarningAndReturnNull_WhenContentLengthIsNegative()
        {
            // Arrange
            var headers = new HttpHeader
            {
                Method = "DELETE",
                Path = "/users",
                Version = "HTTP/1.1"
            };
            headers.Headers["Content-Length"] = "-1";

            using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Hello"));
            using var reader = new StreamReader(memoryStream);

            // Act
            var result = _httpBodyService.ParseHttpBody(reader, headers);

            // Assert
            Assert.That(result, Is.Null);
            _eventService.Received(1).LogEvent(EventType.Warning,
                "Couldn't parse HTTP Request Body: Invalid Content Length",
                Arg.Any<Exception>());
        }
    }
}
