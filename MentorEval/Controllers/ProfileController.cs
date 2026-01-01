using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MentorEval.Interfaces;
using MentorEval.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MentorEval.Services;

namespace MentorEval.Controllers
{
    [Authorize] 
    public class ProfileController : Controller
    {
        private readonly IUserService _userService; 
        private readonly IPasswordService _passwordService;

        public ProfileController(IUserService userService, IPasswordService passwordService)
        {
            _userService = userService;
            _passwordService = passwordService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var viewModel = new UserProfileViewModel
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role
            };

            return View(viewModel);
        }

        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            
            if (user.PasswordHash != model.CurrentPassword) 
            {
                ModelState.AddModelError("CurrentPassword", "Trenutna lozinka nije ispravna");
                return View(model);
            }

            if (!_passwordService.ValidatePassword(model.NewPassword))
            {
                ModelState.AddModelError("NewPassword", "Lozinka mora imati najmanje 6 znakova");
                return View(model);
            }

            user.PasswordHash = model.NewPassword;
            var success = await _userService.UpdateUserAsync(user);

            if (success)
            {
                TempData["Success"] = "Lozinka je uspješno promijenjena!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Greška pri spremanju. Pokušajte ponovo.");
            return View(model);
        }
    }
}