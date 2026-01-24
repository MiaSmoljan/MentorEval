using MentorEval.Interfaces;

namespace MentorEval.Services.TokenStrategies
{

    public class VerificationTokenStrategy : ITokenStrategy
    {
        public string GenerateToken()
        {
            return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        }

        public int GetExpirationMinutes()
        {
            return 60 * 24;
        }

        public string GetTokenType()
        {
            return "EMAIL_VERIFICATION";
        }
    }
}