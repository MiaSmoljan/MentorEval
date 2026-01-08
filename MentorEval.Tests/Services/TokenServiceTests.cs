using MentorEval.Interfaces;
using MentorEval.Services;
using MentorEval.Services.TokenStrategies;
using Moq;
using Xunit;

namespace MentorEval.Tests.Services
{
    public class TokenServiceTests
    {
        [Fact]
        public void GenerateToken_WithVerificationStrategy_ShouldReturnLongToken()
        {
            // Arrange
            var strategy = new VerificationTokenStrategy();
            var tokenService = new TokenService(strategy);

            // Act
            var token = tokenService.GenerateToken();

            // Assert
            Assert.Equal(64, token.Length);
        }

        [Fact]
        public void GenerateToken_WithResetStrategy_ShouldReturnShortToken()
        {
            // Arrange
            var strategy = new PasswordResetTokenStrategy();
            var tokenService = new TokenService(strategy);

            // Act
            var token = tokenService.GenerateToken();

            // Assert
            Assert.Equal(32, token.Length);
        }

        [Fact]
        public void SetStrategy_ShouldChangeTokenGeneration()
        {
            // Arrange
            var tokenService = new TokenService(new VerificationTokenStrategy());

            // Act - prvo generiraj s verification strategijom
            var token1 = tokenService.GenerateToken();
            Assert.Equal(64, token1.Length);

            // Promijeni strategiju
            tokenService.SetStrategy(new PasswordResetTokenStrategy());
            var token2 = tokenService.GenerateToken();

            // Assert - sada treba biti kraći
            Assert.Equal(32, token2.Length);
        }

        [Fact]
        public void GetExpirationMinutes_ShouldReturnStrategyValue()
        {
            // Arrange
            var strategy = new VerificationTokenStrategy();
            var tokenService = new TokenService(strategy);

            // Act
            var minutes = tokenService.GetExpirationMinutes();

            // Assert
            Assert.Equal(1440, minutes);
        }

        [Fact]
        public void GenerateToken_WithMockedStrategy_ShouldUseMock()
        {
            // Arrange - MOCK strategiju
            var mockStrategy = new Mock<ITokenStrategy>();
            mockStrategy.Setup(s => s.GenerateToken()).Returns("MOCKED_TOKEN");
            mockStrategy.Setup(s => s.GetExpirationMinutes()).Returns(999);

            var tokenService = new TokenService(mockStrategy.Object);

            // Act
            var token = tokenService.GenerateToken();
            var expiration = tokenService.GetExpirationMinutes();

            // Assert
            Assert.Equal("MOCKED_TOKEN", token);
            Assert.Equal(999, expiration);
            mockStrategy.Verify(s => s.GenerateToken(), Times.Once);
        }
    }
}