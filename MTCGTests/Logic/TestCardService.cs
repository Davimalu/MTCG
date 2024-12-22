using MTCG.Interfaces.Repository;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Cards;
using MTCG.Models.Enums;
using MTCG.Repository;
using NSubstitute;

namespace MTCGTests.Logic
{
    public class TestCardService
    {
        private CardService _cardService;
        private ICardRepository _cardRepository;

        [SetUp]
        public void SetUp()
        {
            _cardRepository = Substitute.For<ICardRepository>();
            _cardService = new CardService(_cardRepository);
        }

        [Test]
        public void GetCardById_ShouldReturnCard_WhenCardExists()
        {
            // Arrange
            var cardId = "123";
            var expectedCard = new MonsterCard { Id = cardId, Name = "Fire Dragon", Damage = 20, ElementType = ElementType.Fire};
            _cardRepository.GetCardById(cardId).Returns(expectedCard);

            // Act
            var result = _cardService.GetCardById(cardId);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCard));
        }

        [Test]
        public void GetCardById_ShouldReturnNull_WhenCardDoesNotExist()
        {
            // Arrange
            var cardId = "nonexistent";
            _cardRepository.GetCardById(cardId).Returns((Card?)null);

            // Act
            var result = _cardService.GetCardById(cardId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void SaveCardToDatabase_ShouldReturnTrue_WhenCardIsSavedSuccessfully()
        {
            // Arrange
            var card = new SpellCard { Id = "1", Name = "Water Spell", Damage = 100, ElementType = ElementType.Water};
            _cardRepository.AddCardToDatabase(Arg.Any<Card>()).Returns(true);

            // Act
            var result = _cardService.SaveCardToDatabase(card);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void SaveCardToDatabase_ShouldReturnFalse_WhenSaveFails()
        {
            // Arrange
            var card = new SpellCard { Id = "1", Name = "Water Spell", Damage = 100, ElementType = ElementType.Water };
            _cardRepository.AddCardToDatabase(Arg.Any<Card>()).Returns(false);

            // Act
            var result = _cardService.SaveCardToDatabase(card);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void UserOwnsCard_ShouldReturnTrue_WhenUserOwnsCard()
        {
            // Arrange
            var card = new SpellCard { Id = "1", Name = "Water Spell", Damage = 100, ElementType = ElementType.Water };
            var user = new User
            {
                Stack = new Stack() { Cards = new List<Card> { card } }
            };

            // Act
            var result = _cardService.UserOwnsCard(user, card);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void UserOwnsCard_ShouldReturnFalse_WhenUserDoesNotOwnCard()
        {
            // Arrange
            var card = new SpellCard { Id = "1", Name = "Water Spell", Damage = 100, ElementType = ElementType.Water };
            var user = new User
            {
                Stack = new Stack() { Cards = new List<Card>() }
            };

            // Act
            var result = _cardService.UserOwnsCard(user, card);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void UserHasCardInDeck_ShouldReturnTrue_WhenCardIsInDeck()
        {
            // Arrange
            var card = new SpellCard { Id = "1", Name = "Water Spell", Damage = 100, ElementType = ElementType.Water };
            var user = new User
            {
                Deck = new Deck() { Cards = new List<Card> { card } }
            };

            // Act
            var result = _cardService.UserHasCardInDeck(user, card);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void UserHasCardInDeck_ShouldReturnFalse_WhenCardIsNotInDeck()
        {
            // Arrange
            var card = new SpellCard { Id = "1", Name = "Water Spell", Damage = 100, ElementType = ElementType.Water };
            var user = new User
            {
                Deck = new Deck() { Cards = new List<Card>() }
            };

            // Act
            var result = _cardService.UserHasCardInDeck(user, card);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
