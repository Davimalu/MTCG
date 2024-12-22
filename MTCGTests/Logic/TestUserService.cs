using MTCG.Interfaces.Logic;
using MTCG.Interfaces.Repository;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Cards;
using MTCG.Models.Enums;
using NSubstitute;

namespace MTCGTests.Logic
{
    public class TestUserService
    {
        private IUserRepository _userRepository;
        private IStackRepository _stackRepository;
        private IDeckRepository _deckRepository;
        private ICardService _cardService;
        private IEventService _eventService;
        private IUserService _userService;

        [SetUp]
        public void SetUp()
        {
            _userRepository = Substitute.For<IUserRepository>();
            _stackRepository = Substitute.For<IStackRepository>();
            _deckRepository = Substitute.For<IDeckRepository>();
            _cardService = Substitute.For<ICardService>();
            _eventService = Substitute.For<IEventService>();

            _userService = new UserService(_userRepository, _stackRepository, _deckRepository, _cardService, _eventService);
        }

        [Test]
        public void SaveUserToDatabase_ShouldSaveNewUserToDatabase()
        {
            // Arrange
            var user = new User { Id = null, Stack = new Stack(), Deck = new Deck() };
            _userRepository.SaveUserToDatabase(user).Returns(1);

            // Act
            var result = _userService.SaveUserToDatabase(user);

            // Assert
            Assert.That(result, Is.EqualTo(1));
            _userRepository.Received(1).SaveUserToDatabase(user);
        }

        [Test]
        public void SaveUserToDatabase_ShouldUpdateExistingUser()
        {
            // Arrange
            var user = new User { Id = 24, Stack = new Stack(), Deck = new Deck() };
            _userRepository.UpdateUser(user).Returns(24);

            // Act
            var result = _userService.SaveUserToDatabase(user);

            // Assert
            Assert.That(result, Is.EqualTo(24));
            _userRepository.Received(1).UpdateUser(user);
        }

        [Test]
        public void GetUserByName_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = new User { Username = "test", Stack = new Stack(), Deck = new Deck() };
            _userRepository.GetUserByName("test").Returns(user);

            // Act
            var result = _userService.GetUserByName("test");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Username, Is.EqualTo("test"));
        }

        [Test]
        public void GetUserByName_ShouldLogWarningAndReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            _userRepository.GetUserByName("test").Returns((User?)null);

            // Act
            var result = _userService.GetUserByName("test");

            // Assert
            Assert.That(result, Is.Null);
            _eventService.Received(1).LogEvent(EventType.Warning, Arg.Any<string>(), null);
        }

        [Test]
        public void AddStackToUser_ShouldAddCardsToUserStack_WhenCardsExist()
        {
            // Arrange
            var user = new User { Username = "test", Stack = new Stack() };
            var cardIds = new List<string> { "card1", "card2" };
            var card1 = new MonsterCard { Id = cardIds[0] };
            var card2 = new SpellCard { Id = cardIds[1] };

            _stackRepository.GetCardIdsOfUserStack(user).Returns(cardIds);
            _cardService.GetCardById("card1").Returns(card1);
            _cardService.GetCardById("card2").Returns(card2);

            // Act
            var result = _userService.AddStackToUser(user);

            // Arrange
            Assert.That(result.Stack.Cards.Count, Is.EqualTo(2));
            Assert.That(result.Stack.Cards, Does.Contain(card1));
            Assert.That(result.Stack.Cards, Does.Contain(card2));
        }

        [Test]
        public void AddStackToUser_ShouldLogWarning_WhenCardDoesNotExist()
        {
            // Arrange
            var user = new User { Username = "test", Stack = new Stack() };
            var cardIds = new List<string> { "card1" };

            _stackRepository.GetCardIdsOfUserStack(user).Returns(cardIds);
            _cardService.GetCardById("card1").Returns((Card?)null);

            // Act
            var result = _userService.AddStackToUser(user);

            // Assert
            Assert.That(result.Stack.Cards.Count, Is.EqualTo(0));
            _eventService.Received(1).LogEvent(EventType.Warning, Arg.Any<string>(), null);
        }

        [Test]
        public void UserToJson_ShouldReturnSerializedUser()
        {
            // Arrange
            var user = new User
            {
                Username = "test",
                DisplayName = "Test User",
                Biography = "Bio",
                Image = "ImageUrl",
                Stats = new UserStatistics() { Wins = 10, Losses = 5, Ties = 3, EloPoints = 1500 }
            };

            // Act
            var result = _userService.UserToJson(user);

            // Assert
            Assert.That(result, Does.Contain("\"Username\":\"test\""));
            Assert.That(result, Does.Contain("\"DisplayName\":\"Test User\""));
            Assert.That(result, Does.Contain("\"Biography\":\"Bio\""));
            Assert.That(result, Does.Contain("\"Image\":\"ImageUrl\""));
            Assert.That(result, Does.Contain("\"Wins\":10"));
            Assert.That(result, Does.Contain("\"Losses\":5"));
            Assert.That(result, Does.Contain("\"Ties\":3"));
            Assert.That(result, Does.Contain("\"EloPoints\":1500"));
        }

        [Test]
        public void UpdateUserStats_ShouldUpdateStatsCorrectly_WhenWinnerAndLoserProvided()
        {
            // Arrange
            var winner = new User { Id = 12, Stats = new UserStatistics() };
            var loser = new User { Id = 24, Stats = new UserStatistics() };

            // Act
            _userService.UpdateUserStats(winner, loser, false);

            // Assert
            Assert.That(winner.Stats.EloPoints, Is.EqualTo(103));
            Assert.That(winner.Stats.Wins, Is.EqualTo(1));
            Assert.That(loser.Stats.EloPoints, Is.EqualTo(95));
            Assert.That(loser.Stats.Losses, Is.EqualTo(1));

            _userRepository.Received(1).UpdateUser(winner);
            _userRepository.Received(1).UpdateUser(loser);
        }

        [Test]
        public void UpdateUserStats_ShouldUpdateTiesCorrectly_WhenTieIsTrue()
        {
            // Arrange
            var user1 = new User { Id = 28, Stats = new UserStatistics() };
            var user2 = new User { Id = 282, Stats = new UserStatistics() };

            // Act
            _userService.UpdateUserStats(user1, user2, true);

            // Assert
            Assert.That(user1.Stats.Ties, Is.EqualTo(1));
            Assert.That(user2.Stats.Ties, Is.EqualTo(1));

            _userRepository.Received(1).UpdateUser(user1);
            _userRepository.Received(1).UpdateUser(user2);
        }
    }
}
