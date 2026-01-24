namespace MentorEval.Interfaces
{

    public interface ITokenStrategy
    {
        string GenerateToken();
        int GetExpirationMinutes();
        string GetTokenType();
    }

}