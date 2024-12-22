using MTCG.Endpoints;
using MTCG.Interfaces;
using MTCG.Interfaces.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Models;
using NSubstitute;
using System.Text.Json;

namespace MTCGTests.Endpoints
{
    public class TestTradingsEndpoint
    {
        private TradingsEndpoint _endpoint;
        private IUserService _userService;
        private ITradingService _tradingService;
        private ICardService _cardService;
        private IHttpHeaderService _ihttpHeaderService;
        private IEventService _eventService;

        [SetUp]
        public void SetUp()
        {
            _userService = Substitute.For<IUserService>();
            _tradingService = Substitute.For<ITradingService>();
            _cardService = Substitute.For<ICardService>();
            _ihttpHeaderService = Substitute.For<IHttpHeaderService>();
            _eventService = Substitute.For<IEventService>();

            _endpoint = new TradingsEndpoint(_userService, _tradingService, _cardService, _ihttpHeaderService, _eventService);
        }

        [Test]
        public void HandleRequest_GET_UserNotAuthorized_Returns401()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/tradings", Method = "GET", Version = "1.1" };
            _ihttpHeaderService.GetTokenFromHeader(headers).Returns("invalid-token");
            _userService.GetUserByToken("invalid-token").Returns((User?)null);

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(401));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("User not authorized")));
        }

        [Test]
        public void HandleRequest_GET_ListTradeOffers_Returns200WithTradeOffers()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/tradings", Method = "GET", Version = "1.1" };
            var user = new User { Username = "TestUser" };
            _ihttpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(user);

            var tradeDeals = new List<TradeOffer>
            {
                new TradeOffer
                {
                    User = user,
                    Card = new MonsterCard { Id = "card-1", Name = "Dragon", Damage = 50 },
                    RequestedMonster = true,
                    RequestedDamage = 20
                }
            };

            _tradingService.GetAllActiveTradeOffers().Returns(tradeDeals);

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(200));
            Assert.That(result.Item2, Is.Not.Null);
        }

        [Test]
        public void HandleRequest_POST_CreateTradeOffer_EmptyBody_Returns400()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/tradings", Method = "POST", Version = "1.1" };
            var user = new User { Username = "TestUser" };
            _ihttpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(user);

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(400));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Empty request body")));
        }

        [Test]
        public void HandleRequest_DELETE_DeleteTradeOffer_NoTradeFound_Returns400()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/tradings/nonexistent-card-id", Method = "DELETE", Version = "1.1" };
            var user = new User { Username = "TestUser" };
            _ihttpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(user);

            _tradingService.GetTradeOfferByCardId("nonexistent-card-id").Returns((TradeOffer?)null);

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(400));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("There is no trade offer for this card")));
        }

        [Test]
        public void HandleRequest_PUT_InvalidMethod_Returns405()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/tradings", Method = "PUT", Version = "1.1" };
            var user = new User { Username = "TestUser" };
            _ihttpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(user);

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(405));
            Assert.That(result.Item2, Is.EqualTo("Method Not Allowed"));
        }

        [Test]
        public void HandleRequest_POST_CreateTradeOffer_InvalidJson_Returns400()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/tradings", Method = "POST", Version = "1.1" };
            var user = new User { Username = "TestUser" };
            _ihttpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(user);

            var invalidJsonBody = "{ InvalidJson }";

            // Act
            var result = _endpoint.HandleRequest(null, headers, invalidJsonBody);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(400));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Invalid request body")));
        }

        [Test]
        public void HandleRequest_POST_CreateTradeOffer_CardNotOwned_Returns403()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/tradings", Method = "POST", Version = "1.1" };
            var user = new User { Username = "TestUser" };
            _ihttpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(user);

            var validBody = JsonSerializer.Serialize(new { CardToTrade = "card-1", Type = "monster", MinimumDamage = 10 });

            var card = new MonsterCard { Id = "card-1", Name = "Dragon", Damage = 50 };
            _cardService.GetCardById("card-1").Returns(card);
            _cardService.UserOwnsCard(user, card).Returns(false);

            // Act
            var result = _endpoint.HandleRequest(null, headers, validBody);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(403));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("You cannot trade a card you don't own")));
        }

        [Test]
        public void HandleRequest_DeleteTradeOffer_TradeOwnedByAnotherUser_Returns403()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/tradings/card-1", Method = "DELETE", Version = "1.1" };
            var user = new User { Username = "TestUser" };
            var otherUser = new User { Username = "OtherUser" };
            _ihttpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(user);

            var tradeDeal = new TradeOffer
            {
                User = otherUser,
                Card = new MonsterCard { Id = "card-1", Name = "Dragon", Damage = 50 }
            };
            _tradingService.GetTradeOfferByCardId("card-1").Returns(tradeDeal);

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(403));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("You cannot delete other peoples' trade offers!")));
        }

        [Test]
        public void HandleRequest_DeleteTradeOffer_Success_Returns200()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/tradings/card-1", Method = "DELETE", Version = "1.1" };
            var user = new User { Username = "TestUser" };
            _ihttpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(user);

            var tradeDeal = new TradeOffer
            {
                User = user,
                Card = new MonsterCard { Id = "card-1", Name = "Dragon", Damage = 50 }
            };
            _tradingService.GetTradeOfferByCardId("card-1").Returns(tradeDeal);
            _tradingService.RemoveTradeOfferByCardId("card-1").Returns(true);

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(200));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Trade offer removed")));
        }
    }
}
