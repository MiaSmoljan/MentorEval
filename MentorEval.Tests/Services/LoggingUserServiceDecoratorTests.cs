using MentorEval.Interfaces;
using MentorEval.Models;
using MentorEval.Services;
using Moq;
using Xunit;

namespace MentorEval.Tests.Services
{
    public class LoggingUserServiceDecoratorTests
    {
        [Fact]
        public async Task GetUserByIdAsync_ShouldCallInnerService()
        {
            var mockInnerService = new Mock<IUserService>();
            var expectedUser = new User { Id = 1, Username = "testuser", FullName = "Test", Role = "Student", Discriminator = "Student" };
            mockInnerService.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync(expectedUser);

            var decorator = new LoggingUserServiceDecorator(mockInnerService.Object);

            var result = await decorator.GetUserByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
            mockInnerService.Verify(s => s.GetUserByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithNonExistingUser_ShouldReturnNull()
        {
            var mockInnerService = new Mock<IUserService>();
            mockInnerService.Setup(s => s.GetUserByIdAsync(999)).ReturnsAsync((User)null);

            var decorator = new LoggingUserServiceDecorator(mockInnerService.Object);

            var result = await decorator.GetUserByIdAsync(999);

            Assert.Null(result);
            mockInnerService.Verify(s => s.GetUserByIdAsync(999), Times.Once);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ShouldCallInnerService()
        {
            var mockInnerService = new Mock<IUserService>();
            var expectedUser = new User { Id = 1, Username = "john", FullName = "John", Role = "Student", Discriminator = "Student" };
            mockInnerService.Setup(s => s.GetUserByUsernameAsync("john")).ReturnsAsync(expectedUser);

            var decorator = new LoggingUserServiceDecorator(mockInnerService.Object);

            var result = await decorator.GetUserByUsernameAsync("john");

            Assert.NotNull(result);
            Assert.Equal("john", result.Username);
            mockInnerService.Verify(s => s.GetUserByUsernameAsync("john"), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldCallInnerService()
        {
            var mockInnerService = new Mock<IUserService>();
            var user = new User { Id = 1, Username = "test", FullName = "Test", Role = "Student", Discriminator = "Student" };
            mockInnerService.Setup(s => s.UpdateUserAsync(user)).ReturnsAsync(true);

            var decorator = new LoggingUserServiceDecorator(mockInnerService.Object);

            var result = await decorator.UpdateUserAsync(user);

            Assert.True(result);
            mockInnerService.Verify(s => s.UpdateUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenFails_ShouldReturnFalse()
        {
            var mockInnerService = new Mock<IUserService>();
            var user = new User { Id = 1, Username = "test", FullName = "Test", Role = "Student", Discriminator = "Student" };
            mockInnerService.Setup(s => s.UpdateUserAsync(user)).ReturnsAsync(false);

            var decorator = new LoggingUserServiceDecorator(mockInnerService.Object);

            var result = await decorator.UpdateUserAsync(user);

            Assert.False(result);
        }
    }
}