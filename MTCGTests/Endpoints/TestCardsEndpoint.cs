using MTCG.Endpoints;
using MTCG.Interfaces;
using MTCG.Interfaces.HTTP;
using MTCG.Models;
using NSubstitute;
using System.Text.Json;
using MTCG.Interfaces.Logic;

namespace MTCGTests.Endpoints
{
    public class TestCardsEndpoint
    {
        private CardsEndpoint _cardsEndpoint;
        private IUserService _userService;
        private IHttpHeaderService _ihttpHeaderService;

        [SetUp]
        public void Setup()
        {
            _userService = Substitute.For<IUserService>();
            _ihttpHeaderService = Substitute.For<IHttpHeaderService>();

            _cardsEndpoint = new CardsEndpoint(_userService, _ihttpHeaderService);
        }

        [Test]
        public void HandleRequest_GET_UserNotAuthorized_Returns401()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/cards", Method = "GET", Version = "1.1" };
            _ihttpHeaderService.GetTokenFromHeader(headers).Returns(("invalid-token"));
            _userService.GetUserByToken("invalid-token").Returns((User?)null);

            // Act
            var result = _cardsEndpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(401));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("User not authorized")));
        }

        [Test]
        public void HandleRequest_GET_AuthorizedUser_Returns200WithCards()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/cards", Method = "GET", Version = "1.1" };
            var stack = new Stack { Cards = new List<Card> { new MonsterCard { Id = "1", Name = "TestCard" }, new SpellCard { Id = "2", Name = "AnotherTestCard" } } };
            var user = new User { Stack = stack };

            _ihttpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(user);

            // Act
            var result = _cardsEndpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(200));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize(user.Stack)));
        }

        [Test]
        public void HandleRequest_POST_AuthorizedUser_UnsupportedMethod_Returns405()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/cards", Method = "POST", Version = "1.1" };

            _ihttpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(new User());

            // Act
            var result = _cardsEndpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(405));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Method not allowed")));
        }
    }
}
