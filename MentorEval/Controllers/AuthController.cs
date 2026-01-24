using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MentorEval.Models;
using MentorEval.Services;
using MentorEval.Services.TokenStrategies;
using System.Security.Claims;

namespace MentorEval.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private static readonly Dictionary<string, (int Attempts, DateTime LastAttempt)> _loginAttempts = new();
        private const int MaxLoginAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        private void SetNoCacheHeaders()
        {
            Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Append("Pragma", "no-cache");
            Response.Headers.Append("Expires", "0");
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Login()
        {
            SetNoCacheHeaders();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (_loginAttempts.TryGetValue(clientIp, out var attempt) && attempt.Attempts >= MaxLoginAttempts)
            {
                var timeSinceLast = DateTime.UtcNow - attempt.LastAttempt;
                if (timeSinceLast < LockoutDuration)
                {
                    ViewBag.Error = $"Previše neuspješnih pokušaja. Pokušajte za {(LockoutDuration - timeSinceLast).Minutes} minuta.";
                    return View();
                }

                _loginAttempts.Remove(clientIp);
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Unesite korisničko ime i lozinku";
                return View();
            }

            var user = _context.Users
                .FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

            if (user == null)
            {
                _loginAttempts[clientIp] = _loginAttempts.ContainsKey(clientIp)
                    ? (attempt.Attempts + 1, DateTime.UtcNow)
                    : (1, DateTime.UtcNow);

                ViewBag.Error = "Pogrešno korisničko ime ili lozinka";
                return View();
            }

            _loginAttempts.Remove(clientIp);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
#pragma warning disable S4144
        public IActionResult Register()
#pragma warning restore S4144
        {
            SetNoCacheHeaders();
            ViewBag.PageType = "Register";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterUser(string username, string password, string fullName, string email, string role)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fullName))
            {
                ViewBag.Error = "Sva polja su obavezna";
                return View();
            }

            if (_context.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Korisničko ime već postoji";
                return View();
            }

            var user = new User
            {
                Username = username,
                PasswordHash = password,
                FullName = fullName,
                Role = role ?? "Student",
                Discriminator = role ?? "Student"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var tokenService = new TokenService(new VerificationTokenStrategy());
            var verificationToken = tokenService.GenerateToken();
            var expirationMinutes = tokenService.GetExpirationMinutes();

            if (!string.IsNullOrEmpty(email))
            {
                EmailService.SendVerificationEmail(email, verificationToken);

                TempData["Success"] = "Registracija uspješna!";
                TempData["VerificationInfo"] = $"Email: {email}\nToken: {verificationToken.Substring(0, 16)}...\nVrijedi: {expirationMinutes / 60} sati";
            }
            else
            {
                TempData["Success"] = "Registracija uspješna! Možete se prijaviti.";
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
#pragma warning disable S4144
        public IActionResult ForgotPassword()
#pragma warning restore S4144
        {
            SetNoCacheHeaders();
            ViewBag.PageType = "ForgotPassword";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(string email)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Unesite email adresu";
                return View();
            }

            var tokenService = new TokenService(new PasswordResetTokenStrategy());
            var resetToken = tokenService.GenerateToken();
            var expirationMinutes = tokenService.GetExpirationMinutes();

            EmailService.SendPasswordResetEmail(email, resetToken);

            TempData["Success"] = "Email za reset lozinke poslan!";
            TempData["ResetInfo"] = $"Email: {email}\nToken: {resetToken.Substring(0, 16)}...\nVrijedi: {expirationMinutes} minuta";

            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}