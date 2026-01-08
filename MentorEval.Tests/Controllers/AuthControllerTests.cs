using MentorEval.Controllers;
using MentorEval.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace MentorEval.Tests.Controllers
{
    public class AuthControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public void Login_Get_ShouldReturnView()
        {
            var context = GetInMemoryDbContext();
            var controller = new AuthController(context);

            var result = controller.Login();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Login_Post_WithEmptyUsername_ShouldReturnViewWithError()
        {
            var context = GetInMemoryDbContext();
            var controller = new AuthController(context);
            SetupControllerContext(controller);

            var result = await controller.Login("", "password");

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Unesite korisničko ime i lozinku", controller.ViewBag.Error);
        }

        [Fact]
        public async Task Login_Post_WithEmptyPassword_ShouldReturnViewWithError()
        {
            var context = GetInMemoryDbContext();
            var controller = new AuthController(context);
            SetupControllerContext(controller);

            var result = await controller.Login("username", "");

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Unesite korisničko ime i lozinku", controller.ViewBag.Error);
        }

        [Fact]
        public async Task Login_Post_WithInvalidCredentials_ShouldReturnViewWithError()
        {
            var context = GetInMemoryDbContext();
            var controller = new AuthController(context);
            SetupControllerContext(controller);

            var result = await controller.Login("nonexistent", "wrongpassword");

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Pogrešno korisničko ime ili lozinka", controller.ViewBag.Error);
        }

        [Fact]
        public void Register_Get_ShouldReturnView()
        {
            var context = GetInMemoryDbContext();
            var controller = new AuthController(context);

            var result = controller.Register();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Register_Post_WithEmptyFields_ShouldReturnViewWithError()
        {
            var context = GetInMemoryDbContext();
            var controller = new AuthController(context);
            SetupControllerContext(controller);

            var result = await controller.Register("", "password", "", "", "Student");

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Sva polja su obavezna", controller.ViewBag.Error);
        }

        [Fact]
        public async Task Register_Post_WithExistingUsername_ShouldReturnViewWithError()
        {
            var context = GetInMemoryDbContext();
            var existingUser = new User
            {
                Username = "existinguser",
                PasswordHash = "hash",
                FullName = "Existing User",
                Role = "Student",
                Discriminator = "Student"
            };
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var controller = new AuthController(context);
            SetupControllerContext(controller);

            var result = await controller.Register("existinguser", "password", "New User", "test@email.com", "Student");

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Korisničko ime već postoji", controller.ViewBag.Error);
        }

        [Fact]
        public async Task Register_Post_WithValidData_ShouldRedirectToLogin()
        {
            var context = GetInMemoryDbContext();
            var controller = new AuthController(context);
            SetupControllerContext(controller);

            var result = await controller.Register("newuser", "password123", "New User", "new@email.com", "Student");

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task Register_Post_ShouldCreateUserInDatabase()
        {
            var context = GetInMemoryDbContext();
            var controller = new AuthController(context);
            SetupControllerContext(controller);

            await controller.Register("testuser", "password123", "Test User", "test@email.com", "Student");

            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
            Assert.NotNull(user);
            Assert.Equal("Test User", user.FullName);
            Assert.Equal("Student", user.Role);
        }

        [Fact]
        public void ForgotPassword_Get_ShouldReturnView()
        {
            var context = GetInMemoryDbContext();
            var controller = new AuthController(context);

            var result = controller.ForgotPassword();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void ForgotPassword_Post_WithEmptyEmail_ShouldReturnViewWithError()
        {
            var context = GetInMemoryDbContext();
            var controller = new AuthController(context);
            SetupControllerContext(controller);

            var result = controller.ForgotPassword("");

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Unesite email adresu", controller.ViewBag.Error);
        }

        [Fact]
        public void ForgotPassword_Post_WithValidEmail_ShouldRedirectToLogin()
        {
            var context = GetInMemoryDbContext();
            var controller = new AuthController(context);
            SetupControllerContext(controller);

            var result = controller.ForgotPassword("test@email.com");

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        private void SetupControllerContext(Controller controller)
        {
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                httpContext,
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()
            );
        }
    }
}