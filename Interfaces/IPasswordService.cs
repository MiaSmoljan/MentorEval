namespace MentorEval.Interfaces
{
    public interface IPasswordService
    {
        bool ValidatePassword(string password);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}