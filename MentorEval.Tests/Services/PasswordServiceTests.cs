using MentorEval.Services;
using Xunit;

namespace MentorEval.Tests.Services
{
    public class PasswordServiceTests
    {
        private readonly PasswordService _passwordService;

        public PasswordServiceTests()
        {
            _passwordService = new PasswordService();
        }

        [Fact]
        public void ValidatePassword_WithValidPassword_ShouldReturnTrue()
        {
            var password = "password123";

            var result = _passwordService.ValidatePassword(password);

            Assert.True(result);
        }

        [Fact]
        public void ValidatePassword_WithShortPassword_ShouldReturnFalse()
        {
            var password = "12345"; 

            var result = _passwordService.ValidatePassword(password);

            Assert.False(result);
        }

        [Fact]
        public void ValidatePassword_WithEmptyPassword_ShouldReturnFalse()
        {
            var password = "";

            var result = _passwordService.ValidatePassword(password);

            Assert.False(result);
        }

        [Fact]
        public void ValidatePassword_WithNullPassword_ShouldReturnFalse()
        {
            
            string? password = null;

            var result = _passwordService.ValidatePassword(password);

            Assert.False(result);
        }

        [Theory]
        [InlineData("123456", true)]      
        [InlineData("12345", false)]       
        [InlineData("abcdefghij", true)]   
        [InlineData("", false)]           
        public void ValidatePassword_WithVariousInputs_ShouldReturnExpected(string password, bool expected)
        {
            var result = _passwordService.ValidatePassword(password);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void HashPassword_ShouldReturnHashedString()
        {
            var password = "mypassword";

            var hash = _passwordService.HashPassword(password);

            Assert.NotNull(hash);
            Assert.NotEqual(password, hash); 
        }

        [Fact]
        public void HashPassword_SameInput_ShouldReturnSameHash()
        {
            var password = "mypassword";

            var hash1 = _passwordService.HashPassword(password);
            var hash2 = _passwordService.HashPassword(password);

            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
        {
            var password = "mypassword";
            var hash = _passwordService.HashPassword(password);

            var result = _passwordService.VerifyPassword(password, hash);

            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_WithWrongPassword_ShouldReturnFalse()
        {
            var password = "mypassword";
            var wrongPassword = "wrongpassword";
            var hash = _passwordService.HashPassword(password);

            var result = _passwordService.VerifyPassword(wrongPassword, hash);

            Assert.False(result);
        }
    }
}