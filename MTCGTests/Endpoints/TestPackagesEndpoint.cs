using MTCG.Endpoints;
using MTCG.HTTP;
using MTCG.Interfaces;
using MTCG.Models;
using NSubstitute;
using System.Text.Json;

namespace MTCGTests.Endpoints
{
    public class TestPackagesEndpoint
    {
        private PackagesEndpoint _endpoint;
        private ICardService _cardService;
        private IUserService _userService;
        private IPackageService _packageService;
        private IHeaderHelper _headerHelper;
        private IEventService _eventService;

        [SetUp]
        public void SetUp()
        {
            _cardService = Substitute.For<ICardService>();
            _userService = Substitute.For<IUserService>();
            _packageService = Substitute.For<IPackageService>();
            _headerHelper = Substitute.For<IHeaderHelper>();
            _eventService = Substitute.For<IEventService>();

            _endpoint = new PackagesEndpoint(_cardService, _userService, _packageService, _headerHelper, _eventService);
        }

        [Test]
        public void HandleRequest_POST_UserNotAuthorized_Returns401()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/packages", Method = "POST", Version = "1.1" };
            _headerHelper.GetTokenFromHeader(headers).Returns("invalid-token");
            _userService.GetUserByToken("invalid-token").Returns((User?)null);

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(401));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("User not authorized")));
        }

        [Test]
        public void HandleRequest_GET_InvalidMethod_Returns405()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/packages", Method = "GET", Version = "1.1" };
            _headerHelper.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(new User { Username = "admin" });

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(405));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Method Not Allowed")));
        }

        [Test]
        public void HandleRequest_POST_EmptyBody_Returns400()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/packages", Method = "POST", Version = "1.1" };
            _headerHelper.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(new User { Username = "admin" });

            // Act
            var result = _endpoint.HandleRequest(null, headers, null);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(400));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Empty request body")));
        }

        [Test]
        public void HandleRequest_POST_InvalidJson_Returns400()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/packages", Method = "POST", Version = "1.1" };
            _headerHelper.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(new User { Username = "admin" });

            string invalidBody = "Invalid JSON";

            // Act
            var result = _endpoint.HandleRequest(null, headers, invalidBody);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(400));
            Assert.That(result.Item2, Is.EqualTo(JsonSerializer.Serialize("Invalid request body")));
        }

        [Test]
        public void HandleRequest_ValidRequest_CreatesPackageSuccessfully()
        {
            // Arrange
            var headers = new HTTPHeader { Path = "/packages", Method = "POST", Version = "1.1" };
            _headerHelper.GetTokenFromHeader(headers).Returns("valid-token");
            _userService.GetUserByToken("valid-token").Returns(new User { Username = "admin" });

            var cards = new List<Card>
            {
                new MonsterCard { Name = "Card1" },
                new MonsterCard { Name = "Card2" },
                new MonsterCard { Name = "Card3" },
                new MonsterCard { Name = "Card4" },
                new MonsterCard { Name = "Card5" }
            };

            string body = JsonSerializer.Serialize(cards);

            _cardService.SaveCardToDatabase(Arg.Any<Card>()).Returns(true);
            _packageService.AddCardToPackage(Arg.Any<Card>(), Arg.Any<Package>()).Returns(true);
            _packageService.SavePackageToDatabase(Arg.Any<Package>()).Returns(true);

            // Act
            var result = _endpoint.HandleRequest(null, headers, body);

            // Assert
            Assert.That(result.Item1, Is.EqualTo(201));
            Assert.That(result.Item2, Does.Contain("Package created successfully"));
        }
    }
}
