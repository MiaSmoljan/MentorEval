using Microsoft.AspNetCore.Mvc;

namespace MentorEval.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // fix CS8602
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }

        public IActionResult Dashboard()
        {
             //fix CS8602
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }
    }
}