using MentorEval.Controllers;
using MentorEval.Interfaces;
using MentorEval.Models;
using MentorEval.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace MentorEval.Tests.Controllers
{
    public class ProfileControllerTests
    {
        private ProfileController CreateControllerWithUser(Mock<IUserService> mockUserService, Mock<IPasswordService> mockPasswordService, int userId, string username)
        {
            var controller = new ProfileController(mockUserService.Object, mockPasswordService.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext();
            httpContext.User = principal;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                httpContext,
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()
            );

            return controller;
        }

        [Fact]
        public async Task Index_WithValidUser_ShouldReturnViewWithModel()
        {
            var mockUserService = new Mock<IUserService>();
            var mockPasswordService = new Mock<IPasswordService>();
            var user = new User { Id = 1, Username = "testuser", FullName = "Test User", Role = "Student" };
            mockUserService.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync(user);

            var controller = CreateControllerWithUser(mockUserService, mockPasswordService, 1, "testuser");

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserProfileViewModel>(viewResult.Model);
            Assert.Equal("testuser", model.Username);
            Assert.Equal("Test User", model.FullName);
        }

        [Fact]
        public async Task Index_WithNonExistingUser_ShouldRedirectToLogin()
        {
            var mockUserService = new Mock<IUserService>();
            var mockPasswordService = new Mock<IPasswordService>();
            mockUserService.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync((User)null);

            var controller = CreateControllerWithUser(mockUserService, mockPasswordService, 1, "testuser");

            var result = await controller.Index();

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Auth", redirectResult.ControllerName);
        }

        [Fact]
        public void ChangePassword_Get_ShouldReturnViewWithModel()
        {
            var mockUserService = new Mock<IUserService>();
            var mockPasswordService = new Mock<IPasswordService>();
            var controller = CreateControllerWithUser(mockUserService, mockPasswordService, 1, "testuser");

            var result = controller.ChangePassword();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ChangePasswordViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task ChangePassword_Post_WithInvalidModel_ShouldReturnView()
        {
            var mockUserService = new Mock<IUserService>();
            var mockPasswordService = new Mock<IPasswordService>();
            var controller = CreateControllerWithUser(mockUserService, mockPasswordService, 1, "testuser");
            controller.ModelState.AddModelError("Error", "Test error");

            var model = new ChangePasswordViewModel();
            var result = await controller.ChangePassword(model);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task ChangePassword_Post_WithWrongCurrentPassword_ShouldReturnViewWithError()
        {
            var mockUserService = new Mock<IUserService>();
            var mockPasswordService = new Mock<IPasswordService>();
            var user = new User { Id = 1, Username = "testuser", PasswordHash = "correctpassword", FullName = "Test", Role = "Student" };
            mockUserService.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync(user);

            var controller = CreateControllerWithUser(mockUserService, mockPasswordService, 1, "testuser");

            var model = new ChangePasswordViewModel
            {
                CurrentPassword = "wrongpassword",
                NewPassword = "newpassword123",
                ConfirmPassword = "newpassword123"
            };
            var result = await controller.ChangePassword(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ContainsKey("CurrentPassword"));
        }

        [Fact]
        public async Task ChangePassword_Post_WithInvalidNewPassword_ShouldReturnViewWithError()
        {
            var mockUserService = new Mock<IUserService>();
            var mockPasswordService = new Mock<IPasswordService>();
            var user = new User { Id = 1, Username = "testuser", PasswordHash = "currentpassword", FullName = "Test", Role = "Student" };
            mockUserService.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync(user);
            mockPasswordService.Setup(s => s.ValidatePassword("short")).Returns(false);

            var controller = CreateControllerWithUser(mockUserService, mockPasswordService, 1, "testuser");

            var model = new ChangePasswordViewModel
            {
                CurrentPassword = "currentpassword",
                NewPassword = "short",
                ConfirmPassword = "short"
            };
            var result = await controller.ChangePassword(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ContainsKey("NewPassword"));
        }

        [Fact]
        public async Task ChangePassword_Post_WithValidData_ShouldRedirectToIndex()
        {
            var mockUserService = new Mock<IUserService>();
            var mockPasswordService = new Mock<IPasswordService>();
            var user = new User { Id = 1, Username = "testuser", PasswordHash = "currentpassword", FullName = "Test", Role = "Student" };
            mockUserService.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync(user);
            mockPasswordService.Setup(s => s.ValidatePassword("newpassword123")).Returns(true);
            mockUserService.Setup(s => s.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync(true);

            var controller = CreateControllerWithUser(mockUserService, mockPasswordService, 1, "testuser");

            var model = new ChangePasswordViewModel
            {
                CurrentPassword = "currentpassword",
                NewPassword = "newpassword123",
                ConfirmPassword = "newpassword123"
            };
            var result = await controller.ChangePassword(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task ChangePassword_Post_WhenUpdateFails_ShouldReturnViewWithError()
        {
            var mockUserService = new Mock<IUserService>();
            var mockPasswordService = new Mock<IPasswordService>();
            var user = new User { Id = 1, Username = "testuser", PasswordHash = "currentpassword", FullName = "Test", Role = "Student" };
            mockUserService.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync(user);
            mockPasswordService.Setup(s => s.ValidatePassword("newpassword123")).Returns(true);
            mockUserService.Setup(s => s.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync(false);

            var controller = CreateControllerWithUser(mockUserService, mockPasswordService, 1, "testuser");

            var model = new ChangePasswordViewModel
            {
                CurrentPassword = "currentpassword",
                NewPassword = "newpassword123",
                ConfirmPassword = "newpassword123"
            };
            var result = await controller.ChangePassword(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ErrorCount > 0);
        }
    }
}