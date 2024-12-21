using MTCG.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Models.Enums;
using MTCG.Models;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGTests.HTTP
{
    public class TestHttpBodyService
    {
        private HttpBodyService _httpBodyService;
        private IEventService _mockEventService;

        [SetUp]
        public void Setup()
        {
            _mockEventService = Substitute.For<IEventService>();
            _httpBodyService = new HttpBodyService(_mockEventService);
        }

        [Test]
        public void ParseHTTPBody_ShouldReturnBody_WhenContentLengthIsPresent()
        {
            // Arrange
            var headers = new HTTPHeader
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
            var headers = new HTTPHeader
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
            var headers = new HTTPHeader
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
            _mockEventService.Received(1).LogEvent(EventType.Warning,
                "Couldn't parse HTTP Request Body: Invalid Content Length",
                Arg.Any<Exception>());
        }

        [Test]
        public void ParseHTTPBody_ShouldLogWarningAndReturnNull_WhenContentLengthIsNegative()
        {
            // Arrange
            var headers = new HTTPHeader
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
            _mockEventService.Received(1).LogEvent(EventType.Warning,
                "Couldn't parse HTTP Request Body: Invalid Content Length",
                Arg.Any<Exception>());
        }
    }
}
