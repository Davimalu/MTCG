using MTCG.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGTests.HTTP
{
    public class TestHttpResponseService
    {
        private HttpResponseService _service;
        private StreamWriter _mockWriter;
        private MemoryStream _mockStream;

        [SetUp]
        public void SetUp()
        {
            _service = new HttpResponseService();
            _mockStream = new MemoryStream();
            _mockWriter = new StreamWriter(_mockStream) { AutoFlush = true };
        }

        [TearDown]
        public void TearDown()
        {
            _mockWriter.Dispose();
            _mockStream.Dispose();
        }

        [TestCase(200, "OK")]
        [TestCase(201, "Created")]
        [TestCase(400, "Bad Request")]
        [TestCase(401, "Unauthorized")]
        [TestCase(402, "Payment Required")]
        [TestCase(403, "Forbidden")]
        [TestCase(404, "Not Found")]
        [TestCase(405, "Method Not Allowed")]
        [TestCase(409, "Conflict")]
        [TestCase(410, "Gone")]
        [TestCase(500, "Internal Server Error")]
        [TestCase(999, "Unknown")]
        public void SendResponseToClient_SendsCorrectStatusCodeAndReasonPhrase(int statusCode, string expectedReasonPhrase)
        {
            // Arrange
            string? response = null;

            // Act
            _service.SendResponseToClient(_mockWriter, statusCode, response);

            // When data is written to a MemoryStream, the stream's position moves forward
            // By setting the current position of the memory stream back to 0, it allows reading the entire stream from the beginning.
            _mockStream.Position = 0;

            // ReadToEnd reads all the remaining data in the stream from the current position to the end and returns it as a string
            var result = new StreamReader(_mockStream).ReadToEnd();

            // Assert
            StringAssert.Contains($"HTTP/1.1 {statusCode} {expectedReasonPhrase}", result);
        }

        [Test]
        public void SendResponseToClient_SendsContentTypeJson_WhenResponseIsJson()
        {
            // Arrange
            string response = "{\"key\":\"value\"}";

            // Act
            _service.SendResponseToClient(_mockWriter, 200, response);
            _mockStream.Position = 0;
            var result = new StreamReader(_mockStream).ReadToEnd();

            // Assert
            StringAssert.Contains("Content-Type: application/json", result);
        }

        [Test]
        public void SendResponseToClient_SendsContentTypePlainText_WhenResponseIsNotJson()
        {
            // Arrange
            string response = "This is plain text.";

            // Act
            _service.SendResponseToClient(_mockWriter, 200, response);
            _mockStream.Position = 0;
            var result = new StreamReader(_mockStream).ReadToEnd();

            // Assert
            StringAssert.Contains("Content-Type: text/plain", result);
        }

        [Test]
        public void SendResponseToClient_SendsContentLengthBasedOnResponse()
        {
            // Arrange
            string response = "12345";

            // Act
            _service.SendResponseToClient(_mockWriter, 200, response);
            _mockStream.Position = 0;
            var result = new StreamReader(_mockStream).ReadToEnd();

            // Assert
            StringAssert.Contains("Content-Length: 5", result);
        }

        [Test]
        public void SendResponseToClient_SendsContentLengthZero_WhenResponseIsNull()
        {
            // Arrange
            string? response = null;

            // Act
            _service.SendResponseToClient(_mockWriter, 200, response);
            _mockStream.Position = 0;
            var result = new StreamReader(_mockStream).ReadToEnd();

            // Assert
            StringAssert.Contains("Content-Length: 0", result);
        }

        [Test]
        public void SendResponseToClient_SendsResponseBodyCorrectly()
        {
            // Arrange
            string response = "Hello, world!";

            // Act
            _service.SendResponseToClient(_mockWriter, 200, response);
            _mockStream.Position = 0;
            var result = new StreamReader(_mockStream).ReadToEnd();

            // Assert
            StringAssert.Contains(response, result);
        }
    }
}
