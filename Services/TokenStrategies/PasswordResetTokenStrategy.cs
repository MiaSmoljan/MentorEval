using MentorEval.Interfaces;

namespace MentorEval.Services.TokenStrategies
{

    public class PasswordResetTokenStrategy : ITokenStrategy
    {
        public string GenerateToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        public int GetExpirationMinutes()
        {
            return 15;
        }

        public string GetTokenType()
        {
            return "PASSWORD_RESET";
        }
    }
}