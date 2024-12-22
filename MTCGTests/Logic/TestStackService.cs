using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Cards;
using MTCG.Models.Enums;

namespace MTCGTests.Logic
{
    public class TestStackService
    {
        private StackService _stackService;
        private Stack _mockStack;
        private Card _mockCard;
        private Package _mockPackage;

        [SetUp]
        public void SetUp()
        {
            _stackService = StackService.Instance;
            _mockStack = new Stack { Cards = new List<Card>() };
            _mockCard = new MonsterCard { Id = "ahoi2389", Name = "Mock Card", Damage = 20, ElementType = ElementType.Fire };
            _mockPackage = new Package
            {
                Cards = new List<Card>
                {
                    new MonsterCard { Id = "ahoi2389", Name = "Card 1", Damage = 20, ElementType = ElementType.Fire },
                    new SpellCard { Id = "sdg45s", Name = "Card 2", Damage = 30, ElementType = ElementType.Water },
                    new MonsterCard { Id = "sfe4", Name = "Card 3", Damage = 10, ElementType = ElementType.Normal },
                    new SpellCard { Id = "sf4326", Name = "Card 4", Damage = 5, ElementType = ElementType.Fire },
                    new MonsterCard { Id = "j54s", Name = "Card 5", Damage = 70, ElementType = ElementType.Water }
                }
            };
        }

        [Test]
        public void AddCardToStack_ShouldAddCardToStack()
        {
            // Act
            _stackService.AddCardToStack(_mockCard, _mockStack);

            // Assert
            Assert.That(_mockStack.Cards.Contains(_mockCard), Is.True);
        }

        [Test]
        public void AddPackageToStack_ShouldAddAllCardsToStack()
        {
            // Act
            _stackService.AddPackageToStack(_mockPackage, _mockStack);

            // Assert
            Assert.That(_mockStack.Cards.Count, Is.EqualTo(5));
            foreach (var card in _mockPackage.Cards)
            {
                Assert.That(_mockStack.Cards.Contains(card), Is.True);
            }
        }

        [Test]
        public void RemoveCardFromStack_ShouldRemoveCard_WhenCardExistsInStack()
        {
            // Arrange
            _mockStack.Cards.Add(_mockCard);

            // Act
            bool result = _stackService.RemoveCardFromStack(_mockCard, _mockStack);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(_mockStack.Cards.Contains(_mockCard), Is.False);
        }

        [Test]
        public void RemoveCardFromStack_ShouldReturnFalse_WhenCardDoesNotExistInStack()
        {
            // Act
            bool result = _stackService.RemoveCardFromStack(_mockCard, _mockStack);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
