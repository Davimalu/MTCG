using MTCG.Endpoints;
using MTCG.HTTP;
using MTCG.Interfaces;
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
        private IHeaderHelper _headerHelper;
        private TcpClient _tcpClient;

        [SetUp]
        public void SetUp()
        {
            _userService = Substitute.For<IUserService>();
            _battleService = Substitute.For<IBattleService>();
            _headerHelper = Substitute.For<IHeaderHelper>();
            _tcpClient = Substitute.For<TcpClient>();

            _battlesEndpoint = new BattlesEndpoint(_userService, _battleService, _headerHelper);
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
            var headers = new HTTPHeader { Path = "/battles", Method = "POST", Version = "1.1" };
            _headerHelper.GetTokenFromHeader(headers).Returns("invalid-token");
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
            var headers = new HTTPHeader { Path = "/battles", Method = "GET", Version = "1.1" };
            _headerHelper.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(new User { Username = "testUser" });

            // Act
            var result = _battlesEndpoint.HandleRequest(_tcpClient, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(405));
            Assert.That(result.Item2, Is.EqualTo("Method Not Allowed"));
        }
    }
}
