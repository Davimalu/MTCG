using MTCG.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Models;
using MTCG.Models.Enums;
using NSubstitute;

namespace MTCGTests.HTTP
{
    public class TestHttpHeaderService
    {
        private IEventService _eventService;
        private HttpHeaderService _httpHeaderService;

        [SetUp]
        public void SetUp()
        {
            _eventService = Substitute.For<IEventService>();
            _httpHeaderService = new HttpHeaderService(_eventService);
        }

        [Test]
        public void ParseHttpHeader_ReturnsNull_WhenHeaderIsMissing()
        {
            // Arrange

            // https://stackoverflow.com/questions/1879395/how-do-i-generate-a-stream-from-a-string | Second Answer
            // https://www.csharp411.com/c-convert-string-to-stream-and-stream-to-string/
            using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(string.Empty));
            using var reader = new StreamReader(memoryStream);

            // Act
            var result = _httpHeaderService.ParseHttpHeader(reader);

            // Assert
            Assert.That(result, Is.Null);
            _eventService.Received().LogEvent(Arg.Any<EventType>(), Arg.Any<string>(), null);
        }

        [Test]
        public void ParseHttpHeader_ReturnsHttpHeader_WhenHeaderIsValid()
        {
            // Arrange
            string validHeader = "GET /users HTTP/1.1\nHost: example.com\n\n";
            using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(validHeader));
            using var reader = new StreamReader(memoryStream);

            // Act
            var result = _httpHeaderService.ParseHttpHeader(reader);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Method, Is.EqualTo("GET"));
            Assert.That(result.Path, Is.EqualTo("/users"));
            Assert.That(result.Version, Is.EqualTo("HTTP/1.1"));
            Assert.That(result.Headers["Host"], Is.EqualTo("example.com"));
        }

        [Test]
        public void GetTokenFromHeader_ReturnsNull_WhenAuthorizationHeaderIsMissing()
        {
            // Arrange
            var headers = new HTTPHeader
            {
                Path = "/",
                Method = "GET",
                Version = "HTTP/1.1",
                Headers = new Dictionary<string, string>()
            };

            // Act
            var result = _httpHeaderService.GetTokenFromHeader(headers);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetTokenFromHeader_ReturnsToken_WhenAuthorizationHeaderIsValid()
        {
            // Arrange
            var headers = new HTTPHeader
            {
                Path = "/",
                Method = "GET",
                Version = "HTTP/1.1",
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer admin-mtcgToken" }
                }
            };

            // Act
            var result = _httpHeaderService.GetTokenFromHeader(headers);

            // Assert
            Assert.That(result, Is.EqualTo("admin-mtcgToken"));
        }

        [Test]
        public void IsValidAuthorizationField_ReturnsFalse_WhenTokenIsInvalid()
        {
            // Arrange & Act
            var result = _httpHeaderService.IsValidAuthorizationField("InvalidToken");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValidAuthorizationField_ReturnsTrue_WhenTokenIsValid()
        {
            // Arrange & Act
            var result = _httpHeaderService.IsValidAuthorizationField("Bearer admin-mtcgToken");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetQueryParameters_ReturnsEmptyDictionary_WhenNoQueryParametersExist()
        {
            // Arrange
            var headers = new HTTPHeader
            {
                Path = "/deck",
                Method = "GET",
                Version = "HTTP/1.1",
                Headers = new Dictionary<string, string>()
            };

            // Act
            var result = _httpHeaderService.GetQueryParameters(headers);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetQueryParameters_ReturnsDictionary_WhenQueryParametersExist()
        {
            // Arrange
            var headers = new HTTPHeader
            {
                Path = "/deck?format=plain&test=true",
                Method = "GET",
                Version = "HTTP/1.1",
                Headers = new Dictionary<string, string>()
            };

            // Act
            var result = _httpHeaderService.GetQueryParameters(headers);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result["format"], Is.EqualTo("plain"));
            Assert.That(result["test"], Is.EqualTo("true"));
        }

        [Test]
        public void GetPathWithoutQueryParameters_ReturnsPathWithoutQuery()
        {
            // Arrange
            var headers = new HTTPHeader
            {
                Path = "/deck?format=plain",
                Method = "GET",
                Version = "HTTP/1.1",
                Headers = new Dictionary<string, string>()
            };

            // Act
            var result = _httpHeaderService.GetPathWithoutQueryParameters(headers);

            // Assert
            Assert.That(result, Is.EqualTo("/deck"));
        }

        [Test]
        public void GetPathWithoutQueryParameters_ReturnsPath_WhenNoQueryExists()
        {
            // Arrange
            var headers = new HTTPHeader
            {
                Path = "/deck",
                Method = "GET",
                Version = "HTTP/1.1",
                Headers = new Dictionary<string, string>()
            };

            // Act
            var result = _httpHeaderService.GetPathWithoutQueryParameters(headers);

            // Assert
            Assert.That(result, Is.EqualTo("/deck"));
        }
    }
}
