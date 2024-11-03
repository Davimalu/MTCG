using MTCG.Interfaces;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Repository;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MTCGTests
{
    public class TestAuthService
    {
        private IUserRepository userRepositoryMock;
        private AuthService authService;

        [SetUp]
        public void Setup()
        {
            // Create mock for testing
            userRepositoryMock = Substitute.For<IUserRepository>();

            // Define mock behaviour
            userRepositoryMock.GetUserByName("testUser").Returns(new User("testUser", "testPassword"));
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
            var result = authService.Login("anotherTestUser", "doesn'tExist");

            //// Assert ////
            userRepositoryMock.Received(1).GetUserByName(username);
            Assert.Null(result);
        }
    }
}