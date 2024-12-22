using MTCG.Interfaces;
using MTCG.Interfaces.Logic;
using MTCG.Logic;
using MTCG.Models;
using NSubstitute;

namespace MTCGTests.Logic
{
    public class TestBattleService
    {
        private BattleService _battleService;
        private IUserService _userServiceMock;
        private IEventService _eventServiceMock;

        [SetUp]
        public void SetUp()
        {
            _userServiceMock = Substitute.For<IUserService>();
            _eventServiceMock = Substitute.For<IEventService>();

            _battleService = new BattleService(_userServiceMock, _eventServiceMock);
        }


        [Test]
        public void StartBattle_ReturnsBattleLog_WhenBattleCompletes()
        {
            // Arrange
            var playerA = new User
            {
                Username = "PlayerA",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card01", "Goblin", 10, ElementType.Normal) } }
            };

            var playerB = new User
            {
                Username = "PlayerB",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card02", "Ork", 8, ElementType.Normal) } }
            };

            // Act
            var result = _battleService.StartBattle(playerA, playerB);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void StartBattle_HandlesDraws_Correctly()
        {
            // Arrange
            var playerA = new User
            {
                Username = "PlayerA",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card01", "Goblin", 10, ElementType.Normal) } }
            };

            var playerB = new User
            {
                Username = "PlayerB",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card02", "Goblin", 10, ElementType.Normal) } }
            };

            // Act
            var result = _battleService.StartBattle(playerA, playerB);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Contain("None of the players managed to win within 100 rounds"));
        }

        [Test]
        public void StartBattle_HandlesRoundsProperly()
        {
            // Arrange
            var playerA = new User
            {
                Username = "PlayerA",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card01", "Goblin", 10, ElementType.Normal) } }
            };

            var playerB = new User
            {
                Username = "PlayerB",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card02", "Ork", 8, ElementType.Normal) } }
            };

            // Act
            var result = _battleService.StartBattle(playerA, playerB);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Contain("Round 1"));
        }

        [Test]
        public void StartBattle_UpdatesStatsOnWin()
        {
            // Arrange
            var playerA = new User
            {
                Username = "PlayerA",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card01", "Goblin", 10, ElementType.Normal) } }
            };

            var playerB = new User
            {
                Username = "PlayerB",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card02", "Ork", 8, ElementType.Normal) } }
            };

            // Act
            _battleService.StartBattle(playerA, playerB);

            // Assert
            _userServiceMock.Received(1).UpdateUserStats(playerA, playerB, false);
        }

        [Test]
        public void StartBattle_HandlesSpecialCase_GoblinAfraidOfDragon()
        {
            // Arrange
            var playerA = new User
            {
                Username = "PlayerA",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 01","Goblin", 10, ElementType.Normal) } }
            };

            var playerB = new User
            {
                Username = "PlayerB",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 02", "Dragon", 15, ElementType.Fire) } }
            };

            // Act
            var result = _battleService.StartBattle(playerA, playerB);

            // Assert
            Assert.That(result, Does.Contain("The goblin is too afraid of the dragon"));
        }

        [Test]
        public void StartBattle_HandlesSpecialCase_WizardControlsOrk()
        {
            // Arrange
            var playerA = new User
            {
                Username = "PlayerA",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 01", "Wizard", 12, ElementType.Fire) } }
            };

            var playerB = new User
            {
                Username = "PlayerB",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 02", "Ork", 10, ElementType.Normal) } }
            };

            // Act
            var result = _battleService.StartBattle(playerA, playerB);

            // Assert
            Assert.That(result, Does.Contain("The wizard is able to control the ork"));
        }

        [Test]
        public void StartBattle_HandlesSpecialCase_KnightDrownsFromWaterSpell()
        {
            // Arrange
            var playerA = new User
            {
                Username = "PlayerA",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 01", "Knight", 15, ElementType.Normal) } }
            };

            var playerB = new User
            {
                Username = "PlayerB",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 02", "WaterSpell", 10, ElementType.Water) } }
            };

            // Act
            var result = _battleService.StartBattle(playerA, playerB);

            // Assert
            Assert.That(result, Does.Contain("heavy armour pulls him down")); // including the ' in knight's breaks the assertion
        }

        [Test]
        public void StartBattle_HandlesSpecialCase_KrakenImmuneToSpells()
        {
            // Arrange
            var playerA = new User
            {
                Username = "PlayerA",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 01", "Kraken", 20, ElementType.Water) } }
            };

            var playerB = new User
            {
                Username = "PlayerB",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 02", "SpellCard", 15, ElementType.Fire) } }
            };

            // Act
            var result = _battleService.StartBattle(playerA, playerB);

            // Assert
            Assert.That(result, Does.Contain("The kraken is unimpressed as it is immune to spells"));
        }

        [Test]
        public void StartBattle_HandlesSpecialCase_FireElfEvadesDragon()
        {
            // Arrange
            var playerA = new User
            {
                Username = "PlayerA",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 01", "FireElf", 10, ElementType.Fire) } }
            };

            var playerB = new User
            {
                Username = "PlayerB",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 02", "Dragon", 15, ElementType.Fire) } }
            };

            // Act
            var result = _battleService.StartBattle(playerA, playerB);

            // Assert
            Assert.That(result, Does.Contain("The fire elf knows the dragon since they were kids"));
        }

        [Test]
        public void StartBattle_HandlesElementType_WaterVsFire()
        {
            // Arrange
            var playerA = new User
            {
                Username = "PlayerA",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 01", "WaterCard", 10, ElementType.Water) } }
            };

            var playerB = new User
            {
                Username = "PlayerB",
                Deck = new Deck { Cards = new List<Card> { new SpellCard("Card 02", "FireCard", 10, ElementType.Fire) } }
            };

            // Act
            var result = _battleService.StartBattle(playerA, playerB);

            // Assert
            Assert.That(result, Does.Contain("Water is very effective against fire"));
            Assert.That(playerA.Deck.Cards.Count, Is.EqualTo(2));
            Assert.That(playerB.Deck.Cards.Count, Is.EqualTo(0));
        }

        [Test]
        public void StartBattle_HandlesElementType_FireVsNormal()
        {
            // Arrange
            var playerA = new User
            {
                Username = "PlayerA",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 01", "FireCard", 10, ElementType.Fire) } }
            };

            var playerB = new User
            {
                Username = "PlayerB",
                Deck = new Deck { Cards = new List<Card> { new SpellCard("Card 02", "NormalCard", 10, ElementType.Normal) } }
            };

            // Act
            var result = _battleService.StartBattle(playerA, playerB);

            // Assert
            Assert.That(result, Does.Contain("Fire is very effective against normal"));
            Assert.That(playerA.Deck.Cards.Count, Is.EqualTo(2));
            Assert.That(playerB.Deck.Cards.Count, Is.EqualTo(0));
        }

        [Test]
        public void StartBattle_HandlesElementType_NormalVsWater()
        {
            // Arrange
            var playerA = new User
            {
                Username = "PlayerA",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 01", "NormalCard", 10, ElementType.Normal) } }
            };

            var playerB = new User
            {
                Username = "PlayerB",
                Deck = new Deck { Cards = new List<Card> { new SpellCard("Card 02", "WaterCard", 10, ElementType.Water) } }
            };

            // Act
            var result = _battleService.StartBattle(playerA, playerB);

            // Assert
            Assert.That(result, Does.Contain("Normal is very effective against water"));
            Assert.That(playerA.Deck.Cards.Count, Is.EqualTo(2));
            Assert.That(playerB.Deck.Cards.Count, Is.EqualTo(0));
        }

        [Test]
        public void StartBattle_HandlesElementType_WaterVsNormal()
        {
            // Arrange
            var playerA = new User
            {
                Username = "PlayerA",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 01", "WaterCard", 10, ElementType.Water) } }
            };

            var playerB = new User
            {
                Username = "PlayerB",
                Deck = new Deck { Cards = new List<Card> { new SpellCard("Card 02", "NormalCard", 10, ElementType.Normal) } }
            };

            // Act
            var result = _battleService.StartBattle(playerA, playerB);

            // Assert
            Assert.That(result, Does.Contain("Water is not effective against normal"));
            Assert.That(playerB.Deck.Cards.Count, Is.EqualTo(2));
            Assert.That(playerA.Deck.Cards.Count, Is.EqualTo(0));
        }

        [Test]
        public void StartBattle_HandlesElementType_FireVsWater()
        {
            // Arrange
            var playerA = new User
            {
                Username = "PlayerA",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 01", "FireCard", 10, ElementType.Fire) } }
            };

            var playerB = new User
            {
                Username = "PlayerB",
                Deck = new Deck { Cards = new List<Card> { new SpellCard("Card 02", "WaterCard", 10, ElementType.Water) } }
            };

            // Act
            var result = _battleService.StartBattle(playerA, playerB);

            // Assert
            Assert.That(result, Does.Contain("Fire is not effective against water"));
            Assert.That(playerB.Deck.Cards.Count, Is.EqualTo(2));
            Assert.That(playerA.Deck.Cards.Count, Is.EqualTo(0));
        }

        [Test]
        public void StartBattle_HandlesElementType_NormalVsFire()
        {
            // Arrange
            var playerA = new User
            {
                Username = "PlayerA",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 01", "NormalCard", 10, ElementType.Normal) } }
            };

            var playerB = new User
            {
                Username = "PlayerB",
                Deck = new Deck { Cards = new List<Card> { new SpellCard("Card 02", "FireCard", 10, ElementType.Fire) } }
            };

            // Act
            var result = _battleService.StartBattle(playerA, playerB);

            // Assert
            Assert.That(result, Does.Contain("Normal is not effective against fire"));
            Assert.That(playerB.Deck.Cards.Count, Is.EqualTo(2));
            Assert.That(playerA.Deck.Cards.Count, Is.EqualTo(0));
        }

        [Test]
        public void StartBattle_HandlesElementType_MonsterOnlyFight()
        {
            // Arrange
            var playerA = new User
            {
                Username = "PlayerA",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 01", "NormalCard", 10, ElementType.Normal) } }
            };

            var playerB = new User
            {
                Username = "PlayerB",
                Deck = new Deck { Cards = new List<Card> { new MonsterCard("Card 02", "FireCard", 10, ElementType.Fire) } }
            };

            // Act
            var result = _battleService.StartBattle(playerA, playerB);

            // Assert
            Assert.That(result, Does.Contain("None of the players managed to win within 100 rounds"));
            Assert.That(playerB.Deck.Cards.Count, Is.EqualTo(1));
            Assert.That(playerA.Deck.Cards.Count, Is.EqualTo(1));
        }
    }
}
