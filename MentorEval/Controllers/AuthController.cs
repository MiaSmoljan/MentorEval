using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MentorEval.Models;
using System.Security.Claims;

//dotnet ef dbcontext scaffold "Server=.;Database=MentorEvalDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true" Microsoft.EntityFrameworkCore.SqlServer -o Models --context AppDbContext --force


namespace MentorEval.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Unesite korisničko ime i lozinku";
                return View();
            }

            var user = _context.Users
                .FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

            if (user == null)
            {
                ViewBag.Error = "Pogrešno korisničko ime ili lozinka";
                return View();
            }

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

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string fullName, string role)
        {
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

            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}