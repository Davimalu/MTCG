using MTCG.Endpoints;
using MTCG.Interfaces.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Models;
using MTCG.Models.Cards;
using NSubstitute;
using System.Text.Json;
using MTCG.Models.Enums;

namespace MTCGTests.Endpoints
{
    public class TestDeckEndpoint
    {
        private ICardService _cardService;
        private IUserService _userService;
        private IDeckService _deckService;
        private IHttpHeaderService _httpHeaderService;
        private IEventService _eventService;
        private DeckEndpoint _endpoint;

        [SetUp]
        public void SetUp()
        {
            _cardService = Substitute.For<ICardService>();
            _userService = Substitute.For<IUserService>();
            _deckService = Substitute.For<IDeckService>();
            _httpHeaderService = Substitute.For<IHttpHeaderService>();
            _eventService = Substitute.For<IEventService>();

            _endpoint = new DeckEndpoint(
                _cardService,
                _userService,
                _deckService,
                _httpHeaderService,
                _eventService
            );
        }

        [Test]
        public void HandleRequest_GET_UserNotAuthorized_Returns401()
        {
            // Arrange
            var headers = new HttpHeader { Path = "/deck", Method = "GET", Version = "1.1" };
            _httpHeaderService.GetTokenFromHeader(headers).Returns("invalid-token");
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
            var headers = new HttpHeader { Path = "/deck", Method = "GET", Version = "1.1" };
            var user = new User
            {
                Username = "test_user", Deck = new Deck
                {
                    Cards = new List<Card>()
                    {
                        new MonsterCard { Id = "1", Name = "Card1", Damage = 40, ElementType = ElementType.Water},
                        new SpellCard() { Id = "2", Name = "Card2", Damage = 41, ElementType = ElementType.Fire},
                        new SpellCard() { Id = "3", Name = "Card3", Damage = 42, ElementType = ElementType.Normal},
                        new MonsterCard { Id = "4", Name = "Card4", Damage = (float)42.2, ElementType = ElementType.Water}
                    }
                }
            };
            var fancyDeck = new List<FrontendCard>
            {
                new FrontendCard { CardId = "1", CardName = "Card1", Damage = 40, CardType = "Monster", ElementType = "Water"},
                new FrontendCard { CardId = "2", CardName = "Card2", Damage = 41, CardType = "Spell", ElementType = "Fire"},
                new FrontendCard { CardId = "3", CardName = "Card3", Damage = 42, CardType = "Spell", ElementType = "Normal"},
                new FrontendCard { CardId = "4", CardName = "Card4", Damage = (float)42.2, CardType = "Monster", ElementType = "Water"}
            };  
            var serializedFancyDeck = JsonSerializer.Serialize(fancyDeck);

            _httpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(user);
            _httpHeaderService.GetQueryParameters(headers).Returns(new Dictionary<string, string>());
            _cardService.SerializeCardsToJson(user.Deck.Cards).Returns(serializedFancyDeck);

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(200));
            Assert.That(result.Item2, Is.EqualTo(serializedFancyDeck));
            _eventService.Received(1).LogEvent(EventType.Highlight, $"Retrieved deck of user {user.Username} in JSON", null);
        }

        [Test]
        public void HandleRequest_GET_GetDeckWithPlainTextQuery_ReturnsDeckInPlainText()
        {
            // Arrange
            var headers = new HttpHeader { Path = "/deck", Method = "GET", Version = "1.1" };
            var user = new User { Username = "test_user", Deck = new Deck { Cards = new List<Card>() }, Stack = new Stack { Cards = new List<Card> { new MonsterCard { Id = "1" }, new MonsterCard { Id = "2" }, new MonsterCard { Id = "3" }, new MonsterCard { Id = "4" } } } };
            _httpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(user);
            _httpHeaderService.GetQueryParameters(headers).Returns(new Dictionary<string, string> { { "format", "plain" } });
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
            var headers = new HttpHeader { Path = "/deck", Method = "PUT", Version = "1.1" };
            var user = new User { Username = "test_user", Deck = new Deck { Cards = new List<Card>() }, Stack = new Stack { Cards = new List<Card> { new MonsterCard { Id = "1" }, new MonsterCard { Id = "2" }, new MonsterCard { Id = "3" }, new MonsterCard { Id = "4" } } } };
            var body = JsonSerializer.Serialize(new List<string> { "1", "2", "3", "4" });

            _httpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
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
            var headers = new HttpHeader { Path = "/deck", Method = "POST", Version = "1.1" };
            var user = new User { Username = "test_user", Deck = new Deck { Cards = new List<Card>() }, Stack = new Stack { Cards = new List<Card> { new MonsterCard { Id = "1" }, new MonsterCard { Id = "2" }, new MonsterCard { Id = "3" }, new MonsterCard { Id = "4" } } } };
            _httpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(user);

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(405));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Method Not Allowed")));
        }
    }
}
