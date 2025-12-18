using MentorEval.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace MentorEval.Services
{
    public class PasswordService : IPasswordService
    {
        public bool ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            if (password.Length < 6)
                return false;

            return true;
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }
    }
}