using MentorEval.Services.TokenStrategies;
using Xunit;

namespace MentorEval.Tests.Strategies
{
    public class PasswordResetTokenStrategyTests
    {
        private readonly PasswordResetTokenStrategy _strategy;

        public PasswordResetTokenStrategyTests()
        {
            _strategy = new PasswordResetTokenStrategy();
        }

        [Fact]
        public void GenerateToken_ShouldReturnNonEmptyString()
        {
            var token = _strategy.GenerateToken();

            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public void GenerateToken_ShouldReturn32Characters()
        {
            var token = _strategy.GenerateToken();

            Assert.Equal(32, token.Length);
        }

        [Fact]
        public void GenerateToken_ShouldGenerateUniqueTokens()
        {
            var token1 = _strategy.GenerateToken();
            var token2 = _strategy.GenerateToken();

            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void GetExpirationMinutes_ShouldReturn15()
        {
            var minutes = _strategy.GetExpirationMinutes();

            Assert.Equal(15, minutes);
        }

        [Fact]
        public void GetTokenType_ShouldReturnPasswordReset()
        {
            var type = _strategy.GetTokenType();

            Assert.Equal("PASSWORD_RESET", type);
        }
    }
}