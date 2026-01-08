using MentorEval.Models;
using MentorEval.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MentorEval.Tests.Services
{
    public class UserServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithExistingUser_ShouldReturnUser()
        {
            var context = GetInMemoryDbContext();
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = "hash123",
                FullName = "Test User",
                Role = "Student",
                Discriminator = "Student"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userService = new UserService(context);

            var result = await userService.GetUserByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithNonExistingUser_ShouldReturnNull()
        {
            var context = GetInMemoryDbContext();
            var userService = new UserService(context);

            var result = await userService.GetUserByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_WithExistingUser_ShouldReturnUser()
        {
            var context = GetInMemoryDbContext();
            var user = new User
            {
                Username = "johnsmith",
                PasswordHash = "hash",
                FullName = "John Smith",
                Role = "Professor",
                Discriminator = "Professor"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userService = new UserService(context);

            var result = await userService.GetUserByUsernameAsync("johnsmith");

            Assert.NotNull(result);
            Assert.Equal("John Smith", result.FullName);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_WithNonExistingUser_ShouldReturnNull()
        {
            var context = GetInMemoryDbContext();
            var userService = new UserService(context);

            var result = await userService.GetUserByUsernameAsync("nonexistent");

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUser()
        {
            var context = GetInMemoryDbContext();
            var user = new User
            {
                Username = "updatetest",
                PasswordHash = "oldhash",
                FullName = "Old Name",
                Role = "Student",
                Discriminator = "Student"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userService = new UserService(context);

            user.FullName = "New Name";
            var result = await userService.UpdateUserAsync(user);

            Assert.True(result);
            var updatedUser = await context.Users.FindAsync(user.Id);
            Assert.Equal("New Name", updatedUser.FullName);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdatePassword()
        {
            var context = GetInMemoryDbContext();
            var user = new User
            {
                Username = "passtest",
                PasswordHash = "oldpassword",
                FullName = "Test User",
                Role = "Student",
                Discriminator = "Student"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userService = new UserService(context);

            user.PasswordHash = "newpassword";
            var result = await userService.UpdateUserAsync(user);

            Assert.True(result);
            var updatedUser = await context.Users.FindAsync(user.Id);
            Assert.Equal("newpassword", updatedUser.PasswordHash);
        }
    }
}