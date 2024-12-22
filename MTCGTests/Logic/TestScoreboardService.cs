using MTCG.Interfaces;
using MTCG.Interfaces.Logic;
using MTCG.Logic;
using MTCG.Models;
using NSubstitute;

namespace MTCGTests.Logic
{
    public class TestScoreboardService
    {
        private IUserService _userService;
        private ScoreboardService _scoreboardService;

        [SetUp]
        public void SetUp()
        {
            _userService = Substitute.For<IUserService>();
            _scoreboardService = new ScoreboardService(_userService);
        }

        [Test]
        public void FillScoreboard_PopulatesScoreboardWithSortedEntries()
        {
            // Arrange
            var users = new List<string> { "Alice", "Bob", "Charlie" };
            _userService.GetListOfUsers().Returns(users);

            _userService.GetUserByName("Alice").Returns(new User
            {
                Username = "Alice",
                DisplayName = "A",
                Stats = new UserStatistics { EloPoints = 1500, Wins = 10, Losses = 5, Ties = 2 }
            });

            _userService.GetUserByName("Bob").Returns(new User
            {
                Username = "Bob",
                DisplayName = "B",
                Stats = new UserStatistics { EloPoints = 1800, Wins = 15, Losses = 3, Ties = 0 }
            });

            _userService.GetUserByName("Charlie").Returns(new User
            {
                Username = "Charlie",
                DisplayName = "C",
                Stats = new UserStatistics { EloPoints = 1200, Wins = 8, Losses = 10, Ties = 1 }
            });

            var scoreboard = new Scoreboard();

            // Act
            _scoreboardService.FillScoreboard(scoreboard);

            // Assert
            Assert.That(scoreboard.Entries, Has.Count.EqualTo(3));
            Assert.That(scoreboard.Entries[0].Username, Is.EqualTo("Bob"));
            Assert.That(scoreboard.Entries[1].Username, Is.EqualTo("Alice"));
            Assert.That(scoreboard.Entries[2].Username, Is.EqualTo("Charlie"));
        }

        [Test]
        public void FillScoreboard_EmptyUserList_NoEntriesAddedToScoreboard()
        {
            // Arrange
            _userService.GetListOfUsers().Returns(new List<string>());

            var scoreboard = new Scoreboard();

            // Act
            _scoreboardService.FillScoreboard(scoreboard);

            // Assert
            Assert.That(scoreboard.Entries, Is.Empty);
        }

        [Test]
        public void FillScoreboard_HandlesNullUserStatisticsGracefully()
        {
            // Arrange
            var users = new List<string> { "Alice" };
            _userService.GetListOfUsers().Returns(users);

            _userService.GetUserByName("Alice").Returns(new User
            {
                Username = "Alice",
                DisplayName = "A",
                Stats = new UserStatistics() // Simulating null stats
            });

            var scoreboard = new Scoreboard();

            // Act
            _scoreboardService.FillScoreboard(scoreboard);

            // Assert
            Assert.That(scoreboard.Entries, Has.Count.EqualTo(1));
            Assert.That(scoreboard.Entries[0].Username, Is.EqualTo("Alice"));
            Assert.That(scoreboard.Entries[0].EloPoints, Is.EqualTo(100));
            Assert.That(scoreboard.Entries[0].Wins, Is.EqualTo(0));
            Assert.That(scoreboard.Entries[0].Losses, Is.EqualTo(0));
            Assert.That(scoreboard.Entries[0].Ties, Is.EqualTo(0));
        }
    }
}
