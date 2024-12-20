using MTCG.Endpoints;
using MTCG.HTTP;
using MTCG.Interfaces;
using MTCG.Logic;
using MTCG.Models;
using NSubstitute;
using System.Text.Json;

namespace MTCGTests.Endpoints
{
    public class TestScoreboardEndpoint
    {
        private IUserService _userService;
        private IScoreboardService _scoreboardService;
        private IHeaderHelper _headerHelper;
        private ScoreboardEndpoint _endpoint;

        [SetUp]
        public void SetUp()
        {
            _userService = Substitute.For<IUserService>();
            _scoreboardService = Substitute.For<IScoreboardService>();
            _headerHelper = Substitute.For<IHeaderHelper>();
            _endpoint = new ScoreboardEndpoint(_userService, _scoreboardService, _headerHelper);
        }

        [Test]
        public void HandleRequest_GET_UserNotAuthorized_Returns401()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/scoreboard", Method = "GET", Version = "1.1" };
            _headerHelper.GetTokenFromHeader(headers).Returns("invalid_token");
            _userService.GetUserByToken("invalid_token").Returns((User?)null);

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(401));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("User not authorized")));
        }

        [Test]
        public void HandleRequest_POST_MethodNotAllowed_Returns405()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/scoreboard", Method = "POST", Version = "1.1" };
            _headerHelper.GetTokenFromHeader(headers).Returns("valid_token");
            _userService.GetUserByToken("valid_token").Returns(new User());

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(405));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Method Not Allowed")));
        }
    }
}
