﻿using MTCG.Endpoints;
using MTCG.Interfaces.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Models;
using NSubstitute;
using System.Text.Json;

namespace MTCGTests.Endpoints
{
    public class TestScoreboardEndpoint
    {
        private IUserService _userService;
        private IScoreboardService _scoreboardService;
        private IHttpHeaderService _httpHeaderService;
        private ScoreboardEndpoint _endpoint;

        [SetUp]
        public void SetUp()
        {
            _userService = Substitute.For<IUserService>();
            _scoreboardService = Substitute.For<IScoreboardService>();
            _httpHeaderService = Substitute.For<IHttpHeaderService>();
            _endpoint = new ScoreboardEndpoint(_userService, _scoreboardService, _httpHeaderService);
        }

        [Test]
        public void HandleRequest_GET_UserNotAuthorized_Returns401()
        {
            // Arrange
            var headers = new HttpHeader { Path = "/scoreboard", Method = "GET", Version = "1.1" };
            _httpHeaderService.GetTokenFromHeader(headers).Returns("invalid_token");
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
            var headers = new HttpHeader { Path = "/scoreboard", Method = "POST", Version = "1.1" };
            _httpHeaderService.GetTokenFromHeader(headers).Returns("valid_token");
            _userService.GetUserByToken("valid_token").Returns(new User());

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(405));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Method Not Allowed")));
        }
    }
}
