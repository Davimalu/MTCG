using MTCG.Endpoints;
using MTCG.Interfaces;
using MTCG.Interfaces.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Models;
using MTCG.Models.Enums;
using NSubstitute;
using System.Text.Json;
using MTCG.Models.Cards;

namespace MTCGTests.Endpoints
{
    public class TestTransactionsEndpoint
    {
        private IUserService _userService;
        private IPackageService _packageService;
        private IStackService _stackService;
        private IEventService _eventService;
        private IHttpHeaderService _ihttpHeaderService;
        private TransactionsEndpoint _transactionsEndpoint;

        [SetUp]
        public void Setup()
        {
            _userService = Substitute.For<IUserService>();
            _packageService = Substitute.For<IPackageService>();
            _stackService = Substitute.For<IStackService>();
            _eventService = Substitute.For<IEventService>();
            _ihttpHeaderService = Substitute.For<IHttpHeaderService>();

            _transactionsEndpoint = new TransactionsEndpoint(
                _userService,
                _packageService,
                _stackService,
                _eventService,
                _ihttpHeaderService
            );
        }

        [Test]
        public void HandleRequest_POST_UserNotAuthorized_Returns401()
        {
            // Arrange
            var headers = new HttpHeader { Path = "/transactions", Method = "POST", Version = "1.1" };
            _ihttpHeaderService.GetTokenFromHeader(headers).Returns("invalid-token");
            _userService.GetUserByToken("invalid-token").Returns((User?)null);

            // Act
            var result = _transactionsEndpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(401));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("User not authorized")));
        }

        [Test]
        public void HandleRequest_GET_MethodNotAllowed_Returns405()
        {
            // Arrange
            var headers = new HttpHeader { Path = "/transactions", Method = "GET", Version = "1.1" };
            _ihttpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(new User { Username = "test-user" });

            // Act
            var result = _transactionsEndpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(405));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Method Not Allowed")));
        }

        [Test]
        public void HandleRequest_POST_NotEnoughMoney_Returns402()
        {
            // Arrange
            var headers = new HttpHeader { Path = "/transactions/packages", Method = "POST", Version = "1.1" };
            _ihttpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(new User { Username = "test-user", CoinCount = 3 });

            // Act
            var result = _transactionsEndpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(402));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Not enough money")));
        }

        [Test]
        public void HandleRequest_POST_NoPackagesAvailable_Returns410()
        {
            // Arrange
            var headers = new HttpHeader { Path = "/transactions/packages", Method = "POST", Version = "1.1" };
            _ihttpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(new User { Username = "test-user", CoinCount = 10 });
            _packageService.GetRandomPackage().Returns((Package?)null);

            // Act
            var result = _transactionsEndpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(410));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("No packages available")));
        }

        [Test]
        public void HandleRequest_POST_PackageAcquired_Returns201()
        {
            // Arrange
            var headers = new HttpHeader { Path = "/transactions/packages", Method = "POST", Version = "1.1" };
            _ihttpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            var user = new User { Username = "test-user", CoinCount = 10, Stack = new Stack() };
            var package = new Package
            {
                Cards = new List<Card>
                {
                    new MonsterCard { Name = "Monster Card 1" },
                    new MonsterCard { Name = "Monster Card 2" },
                    new MonsterCard { Name = "Monster Card 3" },
                    new MonsterCard { Name = "Monster Card 4" },
                    new MonsterCard { Name = "Monster Card 5" }
                }
            };

            _userService.GetUserByToken("valid-token").Returns(user);
            _packageService.GetRandomPackage().Returns(package);

            // Act
            var result = _transactionsEndpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(201));
            Assert.That(result.Item2, Does.Contain("Package acquired"));

            // Verify methods called
            _stackService.Received(1).AddPackageToStack(package, user.Stack);
            _userService.Received(1).SaveUserToDatabase(user);
            _eventService.Received(1).LogEvent(EventType.Highlight, Arg.Any<string>(), null);
        }


        [Test]
        public void HandleRequest_POST_InvalidPath_Returns404()
        {
            // Arrange
            var headers = new HttpHeader { Path = "/invalid/path", Method = "POST", Version = "1.1" };
            _ihttpHeaderService.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(new User { Username = "test-user", CoinCount = 10 });

            // Act
            var result = _transactionsEndpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(404));
            Assert.That(result.Item2, Is.EqualTo("Not found"));
        }
    }
}
