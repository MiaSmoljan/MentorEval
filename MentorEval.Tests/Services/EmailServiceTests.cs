using MentorEval.Services;
using Xunit;

namespace MentorEval.Tests.Services
{
    public class EmailServiceTests
    {
        [Fact]
        public void Instance_ShouldReturnSameInstance()
        {
            var instance1 = EmailService.Instance;
            var instance2 = EmailService.Instance;

            // SINGLETON test
            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void SendVerificationEmail_ShouldReturnTrue()
        {
            var emailService = EmailService.Instance;

            var result = emailService.SendVerificationEmail("test@email.com", "abc123");
            
            Assert.True(result);
        }

        [Fact]
        public void SendPasswordResetEmail_ShouldReturnTrue()
        {
            var emailService = EmailService.Instance;

            var result = emailService.SendPasswordResetEmail("test@email.com", "xyz789");

            Assert.True(result);
        }

        [Theory]
        [InlineData("user1@test.com", "token123")]
        [InlineData("user2@test.com", "token456")]
        [InlineData("admin@test.com", "token789")]
        public void SendVerificationEmail_WithDifferentInputs_ShouldReturnTrue(string email, string token)
        {
            var emailService = EmailService.Instance;

            var result = emailService.SendVerificationEmail(email, token);

            Assert.True(result);
        }
    }
}