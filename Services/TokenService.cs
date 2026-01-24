using MentorEval.Interfaces;

namespace MentorEval.Services
{

    public class TokenService
    {
        private ITokenStrategy _strategy;

        public TokenService(ITokenStrategy strategy)
        {
            _strategy = strategy;
        }

        public void SetStrategy(ITokenStrategy strategy)
        {
            _strategy = strategy;
            Console.WriteLine($"[TokenService] token type: {strategy.GetTokenType()}");
        }

        public string GenerateToken()
        {
            var token = _strategy.GenerateToken();
            Console.WriteLine($"[TokenService] generiran {_strategy.GetTokenType()} token");
            Console.WriteLine($"[TokenService] vrijedi: {_strategy.GetExpirationMinutes()} minuta");
            return token;
        }

        public int GetExpirationMinutes()
        {
            return _strategy.GetExpirationMinutes();
        }

        public string GetTokenType()
        {
            return _strategy.GetTokenType();
        }
    }
}