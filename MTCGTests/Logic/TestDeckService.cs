using MTCG.Interfaces.Logic;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Enums;
using NSubstitute;

namespace MTCGTests.Logic
{
    public class TestDeckService
    {
        private IEventService _eventService;
        private IDeckService _deckService;

        [SetUp]
        public void SetUp()
        {
            _eventService = Substitute.For<IEventService>();
            _deckService = new DeckService(_eventService);
        }

        [Test]
        public void AddCardToUserDeck_ShouldAddCard_WhenDeckHasLessThanFourCards()
        {
            // Arrange
            var deck = new Deck { Cards = new List<Card>() };
            var card = new MonsterCard { Id = "123", Name = "Fire Dragon", Damage = 20, ElementType = ElementType.Fire };

            // Act
            var result = _deckService.AddCardToUserDeck(card, deck);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(deck.Cards, Contains.Item(card));
        }

        [Test]
        public void AddCardToUserDeck_ShouldReturnFalse_WhenDeckHasFourCards()
        {
            // Arrange
            var deck = new Deck { Cards = new List<Card>() };
            for (int i = 0; i < 4; i++)
            {
                deck.Cards.Add(new SpellCard { Id = $"Bx1{i}2", Name = $"Card {i}", Damage = 2 * i, ElementType = ElementType.Normal });
            }
            var newCard = new MonsterCard { Id = "123", Name = "Fire Dragon", Damage = 20, ElementType = ElementType.Fire };

            // Act
            var result = _deckService.AddCardToUserDeck(newCard, deck);

            // Assert
            Assert.That(result, Is.False);
            _eventService.Received(1).LogEvent(
                EventType.Warning,
                Arg.Is<string>(msg => msg.Contains("Couldn't add card with ID")),
                Arg.Any<Exception>()
            );
        }

        [Test]
        public void RemoveCardFromUserDeck_ShouldRemoveSpecifiedCard()
        {
            // Arrange
            var cardToRemove = new SpellCard { Id = "123", Name = "Card To Remove", Damage = 20, ElementType = ElementType.Fire };
            var deck = new Deck { Cards = new List<Card> { cardToRemove } };

            // Act
            _deckService.RemoveCardFromUserDeck(cardToRemove, deck);

            // Assert
            Assert.That(deck.Cards, Does.Not.Contain(cardToRemove));
        }

        [Test]
        public void RemoveCardFromUserDeck_ShouldDoNothing_WhenCardNotInDeck()
        {
            // Arrange
            var cardToRemove = new SpellCard { Id = "123", Name = "Nonexistent card", Damage = 20, ElementType = ElementType.Fire };
            var deck = new Deck { Cards = new List<Card>() };

            // Act
            _deckService.RemoveCardFromUserDeck(cardToRemove, deck);

            // Assert
            Assert.That(deck.Cards, Is.Empty);
        }

        [Test]
        public void SerializeDeckToPlaintext_ShouldSerializeAllCardsInDeck()
        {
            // Arrange
            var deck = new Deck
            {
                Cards = new List<Card>
                {
                    new MonsterCard { Id = "xqz1", Name = "Dragon", Damage = 50, ElementType = ElementType.Fire },
                    new SpellCard { Id = "xqz2", Name = "Ice Blast", Damage = 30, ElementType = ElementType.Water }
                }
            };

            // Act
            var result = _deckService.SerializeDeckToPlaintext(deck);

            //$"ID: {card.Id}, Name: {card.Name}, Damage: {card.Damage}, Card Type: {(card is MonsterCard ? "Monster Card" : "Spell Card")}, Element Type: {card.ElementType.ToString()}\n";

            // Assert
            Assert.That(result, Does.Contain("ID: xqz1, Name: Dragon, Damage: 50, Card Type: Monster Card, Element Type: Fire"));
            Assert.That(result, Does.Contain("ID: xqz2, Name: Ice Blast, Damage: 30, Card Type: Spell Card, Element Type: Water"));
        }

        [Test]
        public void SerializeDeckToPlaintext_ShouldReturnEmptyString_WhenDeckIsEmpty()
        {
            // Arrange
            var deck = new Deck { Cards = new List<Card>() };

            // Act
            var result = _deckService.SerializeDeckToPlaintext(deck);

            // Assert
            Assert.That(result, Is.Empty);
        }
    }
}
