using MTCG.Interfaces;
using MTCG.Interfaces.Logic;
using MTCG.Interfaces.Repository;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Enums;
using MTCG.Repository;
using NSubstitute;

namespace MTCGTests.Logic
{
    public class TestTradingService
    {
        private ITradeRepository _tradeRepository;
        private IUserService _userService;
        private ICardRepository _cardRepository;
        private IStackService _stackService;
        private IEventService _eventService;
        private TradingService _tradingService;

        [SetUp]
        public void Setup()
        {
            _tradeRepository = Substitute.For<ITradeRepository>();
            _userService = Substitute.For<IUserService>();
            _cardRepository = Substitute.For<ICardRepository>();
            _stackService = Substitute.For<IStackService>();
            _eventService = Substitute.For<IEventService>();

            _tradingService = new TradingService(
                _tradeRepository,
                _userService,
                _cardRepository,
                _stackService,
                _eventService
            );
        }

        [Test]
        public void CreateTradeOffer_UserDoesNotOwnCard_ReturnsNull()
        {
            // Arrange
            var user = new User { Username = "TestUser", Stack = new Stack { Cards = new List<Card>() } };
            var card = new MonsterCard { Id = "card1", Name = "TestCard" };

            // Act
            var result = _tradingService.CreateTradeOffer(user, card, true, 100);

            // Assert
            Assert.That(result, Is.Null);
            _eventService.Received(1).LogEvent(
                EventType.Warning,
                Arg.Is<string>(s => s.Contains("doesn't own Card")),
                Arg.Any<Exception>()
            );
        }

        [Test]
        public void CreateTradeOffer_CardRemovalFails_ReturnsNull()
        {
            // Arrange
            var user = new User
            {
                Username = "TestUser",
                Stack = new Stack { Cards = new List<Card> { new MonsterCard { Id = "card1" } } }
            };
            var card = new MonsterCard { Id = "card1", Name = "TestCard" };

            _stackService.RemoveCardFromStack(Arg.Any<Card>(), Arg.Any<Stack>()).Returns(false);

            // Act
            var result = _tradingService.CreateTradeOffer(user, card, true, 100);

            // Assert
            Assert.That(result, Is.Null);
            _eventService.Received(1).LogEvent(
                EventType.Warning,
                Arg.Is<string>(s => s.Contains("doesn't own Card")),
                Arg.Any<Exception>()
            );
        }

        [Test]
        public void CreateTradeOffer_DatabaseAddFails_ReturnsNull()
        {
            // Arrange
            var user = new User
            {
                Username = "TestUser",
                Stack = new Stack { Cards = new List<Card> { new MonsterCard { Id = "card1" } } }
            };
            var card = new MonsterCard { Id = "card1", Name = "TestCard" };

            _stackService.RemoveCardFromStack(Arg.Any<Card>(), Arg.Any<Stack>()).Returns(true);
            _tradeRepository.AddTradeOfferToDatabase(Arg.Any<TradeOffer>()).Returns((int?)null);

            // Act
            var result = _tradingService.CreateTradeOffer(user, card, true, 100);

            // Assert
            Assert.That(result, Is.Null);
            _eventService.Received(1).LogEvent(
                EventType.Error,
                Arg.Is<string>(s => s.Contains("Database query failed")),
                Arg.Any<Exception>()
            );
        }

        [Test]
        public void CreateTradeOffer_ValidOffer_ReturnsTradeOffer()
        {
            // Arrange
            var user = new User
            {
                Username = "TestUser",
                Stack = new Stack { Cards = new List<Card> { new MonsterCard { Id = "card1" } } }
            };
            var card = new MonsterCard { Id = "card1", Name = "TestCard" };

            _stackService.RemoveCardFromStack(Arg.Any<Card>(), Arg.Any<Stack>()).Returns(true);
            _tradeRepository.AddTradeOfferToDatabase(Arg.Any<TradeOffer>()).Returns(1);
            _tradeRepository.GetAllTradeDeals().Returns(new List<TradeOffer>());

            // Act
            var result = _tradingService.CreateTradeOffer(user, card, true, 100);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.User, Is.EqualTo(user));
            Assert.That(result.Card, Is.EqualTo(card));
        }

        [Test]
        public void RemoveTradeOfferByCardId_TradeOfferDoesNotExist_ReturnsFalse()
        {
            // Arrange
            _tradeRepository.GetTradeDealByCardId(Arg.Any<string>()).Returns((TradeOffer?)null);

            // Act
            var result = _tradingService.RemoveTradeOfferByCardId("card1");

            // Assert
            Assert.That(result, Is.False);
            _eventService.Received(1).LogEvent(
                EventType.Warning,
                Arg.Is<string>(s => s.Contains("Couldn't retrieve trade")),
                Arg.Any<Exception>()
            );
            _eventService.Received(1).LogEvent(
                EventType.Warning,
                Arg.Is<string>(s => s.Contains("Couldn't remove trade")),
                Arg.Any<Exception>()
            );
        }

        [Test]
        public void RemoveTradeOfferByCardId_TradeOfferExists_ReturnsTrue()
        {
            // Arrange
            var tradeOffer = new TradeOffer
            {
                Id = 1,
                Card = new MonsterCard { Id = "card1" },
                User = new User { Id = 3, Username = "TestUser", Stack = new Stack() }
            };

            _tradeRepository.GetTradeDealByCardId("card1").Returns(tradeOffer);
            _tradeRepository.RemoveTradeDeal(tradeOffer).Returns(true);
            _userService.GetUserById(3).Returns(tradeOffer.User);
            _cardRepository.GetCardById("card1").Returns(tradeOffer.Card);

            // Act
            var result = _tradingService.RemoveTradeOfferByCardId("card1");

            // Assert
            Assert.That(result, Is.True);
            _stackService.Received(1).AddCardToStack(tradeOffer.Card, tradeOffer.User.Stack);
            _userService.Received(1).SaveUserToDatabase(tradeOffer.User);
        }

        [Test]
        public void GetAllActiveTradeOffers_PopulationFails_RemovesFaultyOffers()
        {
            // Arrange
            var faultyOffer = new TradeOffer { Card = new MonsterCard { Id = "card1" }, User = new User { Id = 23, Username = "TestUser" } };
            _tradeRepository.GetAllTradeDeals().Returns(new List<TradeOffer> { faultyOffer });

            _userService.GetUserById(Arg.Any<int>()).Returns((User?)null);

            // Act
            var result = _tradingService.GetAllActiveTradeOffers();

            // Assert
            Assert.That(result, Is.Empty);
            _tradeRepository.Received(1).RemoveTradeDeal(Arg.Is<TradeOffer>(offer => offer.Id == faultyOffer.Id)); // The

        }

        [Test]
        public void TryToTrade_NoOtherOffers_ReturnsFalse()
        {
            // Arrange
            var newOffer = new TradeOffer { Card = new MonsterCard { Id = "card1", Damage = 50 }, User = new User { Username = "TestUser" } };
            _tradeRepository.GetAllTradeDeals().Returns(new List<TradeOffer>());

            // Act
            var result = _tradingService.TryToTrade(newOffer);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TryToTrade_SelfTrade_ReturnsFalse()
        {
            // Arrange
            var user = new User { Username = "TestUser" };
            var newOffer = new TradeOffer { Card = new MonsterCard { Id = "card1", Damage = 50 }, User = user, Id = 1 };
            var otherOffer = new TradeOffer { Card = new MonsterCard { Id = "card2", Damage = 60 }, User = user, Id = 2 };

            _tradeRepository.GetAllTradeDeals().Returns(new List<TradeOffer> { newOffer, otherOffer });

            // Act
            var result = _tradingService.TryToTrade(newOffer);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TryToTrade_IncompatibleOffers_InsufficientDamage_ReturnsFalse()
        {
            // Arrange
            var user1 = new User { Id = 1, Username = "User1" };
            var user2 = new User { Id = 2, Username = "User2" };
            var newOffer = new TradeOffer
            {
                Card = new MonsterCard { Id = "card1", Damage = 50 },
                User = user1,
                Id = 1,
                RequestedMonster = false,
                RequestedDamage = 70
            };
            var otherOffer = new TradeOffer
            {
                Card = new SpellCard { Id = "card2", Damage = 60 },
                User = user2,
                Id = 2,
                RequestedMonster = true,
                RequestedDamage = 30
            };

            _tradeRepository.GetAllTradeDeals().Returns(new List<TradeOffer> { newOffer, otherOffer });
            _userService.GetUserById(1).Returns(user1);
            _userService.GetUserById(2).Returns(user2);
            _cardRepository.GetCardById("card1").Returns(newOffer.Card);
            _cardRepository.GetCardById("card2").Returns(otherOffer.Card);

            // Act
            var result = _tradingService.TryToTrade(newOffer);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TryToTrade_IncompatibleOffers_InvalidType_ReturnsFalse()
        {
            // Arrange
            var user1 = new User { Id = 1, Username = "User1" };
            var user2 = new User { Id = 2, Username = "User2" };
            var newOffer = new TradeOffer
            {
                Card = new MonsterCard { Id = "card1", Damage = 50 },
                User = user1,
                Id = 1,
                RequestedMonster = true,
                RequestedDamage = 40
            };
            var otherOffer = new TradeOffer
            {
                Card = new SpellCard { Id = "card2", Damage = 60 },
                User = user2,
                Id = 2,
                RequestedMonster = true,
                RequestedDamage = 30
            };

            _tradeRepository.GetAllTradeDeals().Returns(new List<TradeOffer> { newOffer, otherOffer });
            _userService.GetUserById(1).Returns(user1);
            _userService.GetUserById(2).Returns(user2);
            _cardRepository.GetCardById("card1").Returns(newOffer.Card);
            _cardRepository.GetCardById("card2").Returns(otherOffer.Card);

            // Act
            var result = _tradingService.TryToTrade(newOffer);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TryToTrade_CompatibleOffers_PerformsTrade()
        {
            // Arrange
            var user1 = new User { Id = 1, Username = "User1", Stack = new Stack() };
            var user2 = new User { Id = 2, Username = "User2", Stack = new Stack() };
            var newOffer = new TradeOffer
            {
                Card = new MonsterCard { Id = "card1", Damage = 50 },
                User = user1,
                Id = 1,
                RequestedMonster = false,
                RequestedDamage = 40
            };
            var otherOffer = new TradeOffer
            {
                Card = new SpellCard { Id = "card2", Damage = 60 },
                User = user2,
                Id = 2,
                RequestedMonster = true,
                RequestedDamage = 30
            };

            _tradeRepository.GetAllTradeDeals().Returns(new List<TradeOffer> { newOffer, otherOffer });
            _userService.GetUserById(1).Returns(user1);
            _userService.GetUserById(2).Returns(user2);
            _cardRepository.GetCardById("card1").Returns(newOffer.Card);
            _cardRepository.GetCardById("card2").Returns(otherOffer.Card);

            _tradeRepository.SetTradeOfferInactive(1).Returns(true);
            _tradeRepository.SetTradeOfferInactive(2).Returns(true);

            // Act
            var result = _tradingService.TryToTrade(newOffer);

            // Assert
            Assert.That(result, Is.True);
            _stackService.Received(1).AddCardToStack(newOffer.Card, user2.Stack);
            _stackService.Received(1).AddCardToStack(otherOffer.Card, user1.Stack);
            _userService.Received(1).SaveUserToDatabase(user1);
            _userService.Received(1).SaveUserToDatabase(user2);
            _eventService.Received(2).LogEvent(
                EventType.Highlight,
                Arg.Is<string>(s => s.Contains("received card")),
                Arg.Any<Exception>()
            );
        }
    }
}
