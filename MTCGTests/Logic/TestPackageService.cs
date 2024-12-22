using MTCG.Interfaces.Repository;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Cards;
using MTCG.Models.Enums;
using NSubstitute;

namespace MTCGTests.Logic
{
    public class TestPackageService
    {
        private IPackageRepository _packageRepository;
        private PackageService _packageService;

        [SetUp]
        public void Setup()
        {
            _packageRepository = Substitute.For<IPackageRepository>();
            _packageService = new PackageService(_packageRepository);
        }

        [Test]
        public void AddCardToPackage_ShouldReturnFalse_WhenPackageIsFull()
        {
            // Arrange
            var card = new MonsterCard { Id = "bxz2", Name = "Fire Dragon", Damage = 20, ElementType = ElementType.Fire };
            var package = new Package { Cards = new List<Card>(new MonsterCard[5]) }; // Package already has 5 cards

            // Act
            var result = _packageService.AddCardToPackage(card, package);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void AddCardToPackage_ShouldReturnTrue_WhenCardAddedSuccessfully()
        {
            // Arrange
            var card = new MonsterCard { Id = "bxz2", Name = "Fire Dragon", Damage = 20, ElementType = ElementType.Fire };
            var package = new Package { Cards = new List<Card>() }; // Package is empty

            // Act
            var result = _packageService.AddCardToPackage(card, package);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(package.Cards.Contains(card), Is.True);
        }

        [Test]
        public void SavePackageToDatabase_ShouldReturnTrue_WhenRepositoryReturnsTrue()
        {
            // Arrange
            var package = new Package { Cards = new List<Card>() };
            _packageRepository.AddPackageToDatabase(package).Returns(true);

            // Act
            var result = _packageService.SavePackageToDatabase(package);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void SavePackageToDatabase_ShouldReturnFalse_WhenRepositoryReturnsFalse()
        {
            // Arrange
            var package = new Package { Cards = new List<Card>() };
            _packageRepository.AddPackageToDatabase(package).Returns(false);

            // Act
            var result = _packageService.SavePackageToDatabase(package);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetRandomPackage_ShouldReturnPackage_WhenPackageExists()
        {
            // Arrange
            var package = new Package { Cards = new List<Card>() };
            _packageRepository.GetIdOfRandomPackage().Returns(1);
            _packageRepository.GetPackageById(1).Returns(package);

            // Act
            var result = _packageService.GetRandomPackage();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(package));
            _packageRepository.Received(1).DeletePackageById(1);
        }

        [Test]
        public void GetRandomPackage_ShouldReturnNull_WhenNoPackageExists()
        {
            // Arrange
            _packageRepository.GetIdOfRandomPackage().Returns(1);
            _packageRepository.GetPackageById(1).Returns((Package?)null);

            // Act
            var result = _packageService.GetRandomPackage();

            // Assert
            Assert.That(result, Is.Null);
        }
    }
}
