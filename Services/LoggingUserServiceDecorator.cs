using MentorEval.Interfaces;
using MentorEval.Models;

namespace MentorEval.Services
{

    public class LoggingUserServiceDecorator : IUserService
    {
        private readonly IUserService _innerService;

        public LoggingUserServiceDecorator(IUserService innerService)
        {
            _innerService = innerService;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            Console.WriteLine($"[LOG] {DateTime.Now:HH:mm:ss} | GetUserByIdAsync STARTED | ID: {id}");

            var user = await _innerService.GetUserByIdAsync(id);

            if (user != null)
                Console.WriteLine($"[LOG] {DateTime.Now:HH:mm:ss} | GetUserByIdAsync SUCCESS | User: {user.Username}");
            else
                Console.WriteLine($"[LOG] {DateTime.Now:HH:mm:ss} | GetUserByIdAsync FAILED | User not found");

            return user;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            Console.WriteLine($"[LOG] {DateTime.Now:HH:mm:ss} | GetUserByUsernameAsync STARTED | Username: {username}");

            var user = await _innerService.GetUserByUsernameAsync(username);

            Console.WriteLine($"[LOG] {DateTime.Now:HH:mm:ss} | GetUserByUsernameAsync {(user != null ? "SUCCESS" : "FAILED")}");

            return user;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            Console.WriteLine($"[LOG] {DateTime.Now:HH:mm:ss} | UpdateUserAsync STARTED | User: {user.Username}");

            var result = await _innerService.UpdateUserAsync(user);

            Console.WriteLine($"[LOG] {DateTime.Now:HH:mm:ss} | UpdateUserAsync {(result ? "SUCCESS" : "FAILED")}");

            return result;
        }
    }
}