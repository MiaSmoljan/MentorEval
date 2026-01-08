using MentorEval.Services.TokenStrategies;
using Xunit;

namespace MentorEval.Tests.Strategies
{
    public class VerificationTokenStrategyTests
    {
        private readonly VerificationTokenStrategy _strategy;

        public VerificationTokenStrategyTests()
        {
            _strategy = new VerificationTokenStrategy();
        }

        [Fact]
        public void GenerateToken_ShouldReturnNonEmptyString()
        {
            var token = _strategy.GenerateToken();

            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public void GenerateToken_ShouldReturn64Characters()
        {
            var token = _strategy.GenerateToken();

            Assert.Equal(64, token.Length);
        }

        [Fact]
        public void GenerateToken_ShouldGenerateUniqueTokens()
        {
            var token1 = _strategy.GenerateToken();
            var token2 = _strategy.GenerateToken();

            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void GetExpirationMinutes_ShouldReturn1440()
        {
            var minutes = _strategy.GetExpirationMinutes();

            Assert.Equal(1440, minutes);
        }

        [Fact]
        public void GetTokenType_ShouldReturnEmailVerification()
        {
            var type = _strategy.GetTokenType();

            Assert.Equal("EMAIL_VERIFICATION", type);
        }
    }
}