﻿using MTCG.Endpoints;
using MTCG.Interfaces.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Models;
using NSubstitute;
using System.Text.Json;

namespace MTCGTests.Endpoints
{
    public class TestStatsEndpoint
    {
        private IUserService _userService;
        private IHttpHeaderService _httpHeaderService;
        private StatsEndpoint _endpoint;

        [SetUp]
        public void SetUp()
        {
            _userService = Substitute.For<IUserService>();
            _httpHeaderService = Substitute.For<IHttpHeaderService>();
            _endpoint = new StatsEndpoint(_userService, _httpHeaderService);
        }

        [Test]
        public void HandleRequest_POST_UserNotAuthorized_Returns401()
        {
            // Arrange
            var headers = new HttpHeader { Path = "/stats", Method = "POST", Version = "1.1" };
            _httpHeaderService.GetTokenFromHeader(headers).Returns((string?)null);

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(401));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("User not authorized")));
        }

        [Test]
        public void HandleRequest_GET_UserAuthorized_ReturnsStats()
        {
            // Arrange
            var headers = new HttpHeader { Path = "/stats", Method = "GET", Version = "1.1" };
            var user = new User { Stats = new UserStatistics { Wins = 10, Losses = 5 } };

            _httpHeaderService.GetTokenFromHeader(headers).Returns("valid_token");
            _userService.GetUserByToken("valid_token").Returns(user);

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(200));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize(user.Stats)));
        }

        [Test]
        public void HandleRequest_POST_InvalidMethod_UserAuthorized_Returns405()
        {
            // Arrange
            var headers = new HttpHeader { Path = "/stats", Method = "POST", Version = "1.1" };
            var user = new User { Stats = new UserStatistics { Wins = 10, Losses = 5 } };

            _httpHeaderService.GetTokenFromHeader(headers).Returns("valid_token");
            _userService.GetUserByToken("valid_token").Returns(user);

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(405));
            Assert.That(result.Item2, Is.EqualTo("Method not allowed"));
        }
    }
}
