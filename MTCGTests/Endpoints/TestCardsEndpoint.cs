using MTCG.Endpoints;
using MTCG.Interfaces.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Models;
using MTCG.Models.Cards;
using MTCG.Models.Enums;
using NSubstitute;
using System.Text.Json;

namespace MTCGTests.Endpoints
{
    public class TestCardsEndpoint
    {
        private CardsEndpoint _cardsEndpoint;
        private IUserService _userService;
        private IHttpHeaderService _httpHeaderService;

        [SetUp]
        public void Setup()
        {
            _userService = Substitute.For<IUserService>();
            _httpHeaderService = Substitute.For<IHttpHeaderService>();

            _cardsEndpoint = new CardsEndpoint(_userService, _httpHeaderService);
        }

        [Test]
        public void HandleRequest_GET_UserNotAuthorized_Returns401()
        {
            // Arrange
            var headers = new HttpHeader { Path = "/cards", Method = "GET", Version = "1.1" };
            _httpHeaderService.GetTokenFromHeader(headers).Returns(("invalid-token"));
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
            var headers = new HttpHeader { Path = "/cards", Method = "GET", Version = "1.1" };
            var stack = new Stack
            {
                Cards = new List<Card>
                {
                    new MonsterCard
                    {
                        Id = "1", Name = "TestCard", Damage = 42, ElementType = ElementType.Water
                    },
                    new SpellCard
                    {
                        Id = "2", Name = "AnotherTestCard", Damage = (float)42.4, ElementType = ElementType.Fire
                    }
                }
            };
            var user = new User { Username = "TestUser", Stack = stack };

            var fancyStack = new List<FrontendCard>
            {
                new FrontendCard { CardId = "1", CardName = "TestCard", Damage = 42, CardType = "Monster Card", ElementType = "Water"},
                new FrontendCard { CardId = "2", CardName = "AnotherTestCard", Damage = (float)42.4, CardType = "Spell Card", ElementType = "Fire"},
            };
            var serializedFancyDeck = JsonSerializer.Serialize(fancyStack);


            _httpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(user);

            // Act
            var result = _cardsEndpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(200));
            Assert.That(result.Item2, Is.EqualTo(serializedFancyDeck));
        }

        [Test]
        public void HandleRequest_POST_AuthorizedUser_UnsupportedMethod_Returns405()
        {
            // Arrange
            var headers = new HttpHeader { Path = "/cards", Method = "POST", Version = "1.1" };

            _httpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(new User());

            // Act
            var result = _cardsEndpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(405));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Method not allowed")));
        }
    }
}
