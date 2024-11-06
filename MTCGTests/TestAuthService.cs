using MTCG.Interfaces;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Repository;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using BCrypt.Net;

namespace MTCGTests
{
    using BCrypt.Net;

    public class TestAuthService
    {
        private IUserRepository userRepositoryMock;
        private AuthService authService;

        [SetUp]
        public void Setup()
        {
            var hashedPassword = BCrypt.HashPassword("testPassword");

            // Create mock for testing
            userRepositoryMock = Substitute.For<IUserRepository>();

            // Define mock behaviour
            userRepositoryMock.GetUserByName("testUser").Returns(new User("testUser", hashedPassword));
            userRepositoryMock.GetUserByName("anotherTestUser").ReturnsNull();

            // Dependency Injection in AuthService
            authService = new AuthService(userRepositoryMock);
        }

        [Test]
        public void testRegister_ReturnsFalseIfUserAlreadyExists()
        {
            //// Arrange ////


            //// Act ////
            var result = authService.Register("testUser", "testUserAlreadyExists");

            //// Assert ////
            Assert.False(result);
        }

        [Test]
        public void testRegister_ReturnsTrueIfUserDoesntExist()
        {
            //// Arrange ////


            //// Act ////
            var result = authService.Register("anotherTestUser", "doesn'tExist");

            //// Assert ////
            Assert.True(result);
        }

        [Test]
        public void testRegister_AddsNewUserToDatabase()
        {
            //// Arrange ////

            //// Act ////
            var result = authService.Register("anotherTestUser", "doesn'tExist");

            //// Assert ////

            // Use Arg.Is - otherwise the mock will check if it’s the exact same instance of the user
            userRepositoryMock.Received(1).AddUser(Arg.Is<User>(u => u.Username == "anotherTestUser"));
        }

        [Test]
        public void testLogin_ReturnsNullIfUserDoesntExist()
        {
            //// Arrange ////
            var username = "anotherTestUser";

            //// Act ////
            var result = authService.Login(username, "doesn'tExist");

            //// Assert ////
            userRepositoryMock.Received(1).GetUserByName(username);
            Assert.Null(result);
        }

        [Test]
        public void testLogin_ReturnsNullIfPasswordIsInvalid()
        {
            //// Arrange ////
            var username = "testUser";
            var password = "wrongPassword";

            //// Act ////
            var result = authService.Login(username, password);

            //// Assert ////
            userRepositoryMock.Received(1).GetUserByName(username);
            Assert.Null(result);
        }
        
        [Test]
        public void testLogin_ReturnsUserIfPasswordIsValid()
        {
            //// Arrange ////
            var username = "testUser";
            var password = "testPassword"; // Correct password

            //// Act ////
            var result = authService.Login(username, password);

            //// Assert ////
            userRepositoryMock.Received(1).GetUserByName(username);
            Assert.NotNull(result);
            Assert.That(username, Is.EqualTo(result.Username), "Should return the user that logged in");
        }
    }
}