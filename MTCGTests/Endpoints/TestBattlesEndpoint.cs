using MTCG.Endpoints;
using MTCG.Interfaces.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Models;
using NSubstitute;
using System.Net.Sockets;

namespace MTCGTests.Endpoints
{
    public class TestBattlesEndpoint
    {
        private BattlesEndpoint _battlesEndpoint;
        private IUserService _userService;
        private IBattleService _battleService;
        private IHttpHeaderService _httpHeaderService;
        private TcpClient _tcpClient;

        [SetUp]
        public void SetUp()
        {
            _userService = Substitute.For<IUserService>();
            _battleService = Substitute.For<IBattleService>();
            _httpHeaderService = Substitute.For<IHttpHeaderService>();
            _tcpClient = Substitute.For<TcpClient>();

            _battlesEndpoint = new BattlesEndpoint(_userService, _battleService, _httpHeaderService);
        }

        [TearDown]
        public void TearDown()
        {
            _tcpClient?.Dispose();
        }

        [Test]
        public void HandleRequest_POST_UserNotAuthorized_ReturnsUnauthorized()
        {
            // Arrange
            var headers = new HttpHeader { Path = "/battles", Method = "POST", Version = "1.1" };
            _httpHeaderService.GetTokenFromHeader(headers).Returns("invalid-token");
            _userService.GetUserByToken("valid-token").Returns((User?)null);

            // Act
            var result = _battlesEndpoint.HandleRequest(_tcpClient, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(401));
            Assert.That(result.Item2, Is.EqualTo("User not authorized"));
        }

        [Test]
        public void HandleRequest_GET_MethodNotAllowed_ReturnsMethodNotAllowed()
        {
            // Arrange
            var headers = new HttpHeader { Path = "/battles", Method = "GET", Version = "1.1" };
            _httpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(new User { Username = "testUser" });

            // Act
            var result = _battlesEndpoint.HandleRequest(_tcpClient, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(405));
            Assert.That(result.Item2, Is.EqualTo("Method Not Allowed"));
        }
    }
}
