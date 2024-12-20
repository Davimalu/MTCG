using MTCG.Endpoints;
using MTCG.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Interfaces;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MTCG.Models;

namespace MTCGTests.Endpoints
{
    public class TestDeckEndpoint
    {
        private ICardService _cardService;
        private IUserService _userService;
        private IDeckService _deckService;
        private IHeaderHelper _headerHelper;
        private IEventService _eventService;
        private DeckEndpoint _endpoint;

        [SetUp]
        public void SetUp()
        {
            _cardService = Substitute.For<ICardService>();
            _userService = Substitute.For<IUserService>();
            _deckService = Substitute.For<IDeckService>();
            _headerHelper = Substitute.For<IHeaderHelper>();
            _eventService = Substitute.For<IEventService>();

            _endpoint = new DeckEndpoint(
                _cardService,
                _userService,
                _deckService,
                _headerHelper,
                _eventService
            );
        }

        [Test]
        public void HandleRequest_GET_UserNotAuthorized_Returns401()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/deck", Method = "GET", Version = "1.1" };
            _headerHelper.GetTokenFromHeader(headers).Returns("invalid-token");
            _userService.GetUserByToken("invalid-token").Returns((User?)null);

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(401));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("User not authorized")));
        }

        [Test]
        public void HandleRequest_GET_GetDeckWithoutQueryParameters_ReturnsDeckInJson()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/deck", Method = "GET", Version = "1.1" };
            var user = new User { Username = "test_user", Deck = new Deck { Cards = new List<Card>() }, Stack = new Stack { Cards = new List<Card> { new MonsterCard { Id = "1" }, new MonsterCard { Id = "2" }, new MonsterCard { Id = "3" }, new MonsterCard { Id = "4" } } } };
            _headerHelper.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(user);
            _headerHelper.GetQueryParameters(headers).Returns(new Dictionary<string, string>());

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(200));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize(user.Deck.Cards)));
        }

        [Test]
        public void HandleRequest_GET_GetDeckWithPlainTextQuery_ReturnsDeckInPlainText()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/deck", Method = "GET", Version = "1.1" };
            var user = new User { Username = "test_user", Deck = new Deck { Cards = new List<Card>() }, Stack = new Stack { Cards = new List<Card> { new MonsterCard { Id = "1" }, new MonsterCard { Id = "2" }, new MonsterCard { Id = "3" }, new MonsterCard { Id = "4" } } } };
            _headerHelper.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(user);
            _headerHelper.GetQueryParameters(headers).Returns(new Dictionary<string, string> { { "format", "plain" } });
            _deckService.SerializeDeckToPlaintext(user.Deck).Returns("Card 1\nCard 2\nCard 3\nCard 4");

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(200));
            Assert.That(result.Item2, Is.EqualTo("Card 1\nCard 2\nCard 3\nCard 4"));
        }

        [Test]
        public void HandleRequest_PUT_PutDeckWithValidBody_UpdatesDeckAndReturns200()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/deck", Method = "PUT", Version = "1.1" };
            var user = new User { Username = "test_user", Deck = new Deck { Cards = new List<Card>() }, Stack = new Stack { Cards = new List<Card> { new MonsterCard { Id = "1" }, new MonsterCard { Id = "2" }, new MonsterCard { Id = "3" }, new MonsterCard { Id = "4" } } } };
            var body = JsonSerializer.Serialize(new List<string> { "1", "2", "3", "4" });

            _headerHelper.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(user);

            // Lambda expression that dynamically creates a new Card object. The Id property of the Card is set to the value of the argument passed to the GetCardById method.
            _cardService.GetCardById(Arg.Any<string>()).Returns(x => new MonsterCard { Id = x.Arg<string>() });

            // Act
            var result = _endpoint.HandleRequest(null, headers, body);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(200));
            Assert.That(result.Item2, Does.Contain("Deck updated"));
        }

        [Test]
        public void HandleRequest_POST_InvalidHttpMethod_Returns405()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/deck", Method = "POST", Version = "1.1" };
            var user = new User { Username = "test_user", Deck = new Deck { Cards = new List<Card>() }, Stack = new Stack { Cards = new List<Card> { new MonsterCard { Id = "1" }, new MonsterCard { Id = "2" }, new MonsterCard { Id = "3" }, new MonsterCard { Id = "4" } } } };
            _headerHelper.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(user);

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(405));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Method Not Allowed")));
        }
    }
}
