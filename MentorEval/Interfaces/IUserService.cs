using MentorEval.Models;

namespace MentorEval.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> UpdateUserAsync(User user);
    }
}