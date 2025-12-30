using MentorEval.Models;
using MentorEval.Models.ViewModels;
using MentorEval.Services.Evaluations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MentorEval.Controllers
{
    [Authorize(Roles = "Professor")]
    public class EvaluationsController : Controller
    {
        private readonly AppDbContext _context;

        private readonly IEvaluationCreationService _creation;

        private readonly EvaluationFacade _facade;
        public EvaluationsController(AppDbContext context, EvaluationFacade facade)
        {
            _context = context;
            _facade = facade;
        }


        private int GetCurrentProfessorId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(idStr);
        }

        public async Task<IActionResult> Index()
        {
            int profId = GetCurrentProfessorId();

            var evaluations = await _context.Evaluations
                .Include(e => e.Course)
                .Where(e => e.Course.ProfessorId == profId)
                .OrderByDescending(e => e.StartAt)
                .ToListAsync();

            var now = DateTime.Now;
            int activeCount = evaluations
                .Count(e => e.Status == "Active"
                            && e.StartAt <= now
                            && e.EndAt >= now);

            ViewBag.ActiveEvaluationsCount = activeCount;

            return View(evaluations);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            int profId = GetCurrentProfessorId();
            var courses = await _facade.GetProfessorCoursesAsync(profId);

            var vm = new EvaluationCreateViewModel
            {
                AvailableCourses = courses.Select(c => new CourseOption { Id = c.Id, Name = c.Name }).ToList(),
                Questions = new List<QuestionCreateViewModel>()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EvaluationCreateViewModel vm)
        {
            int profId = GetCurrentProfessorId();

            if (!ModelState.IsValid)
            {
                var courses = await _facade.GetProfessorCoursesAsync(profId);
                vm.AvailableCourses = courses.Select(c => new CourseOption { Id = c.Id, Name = c.Name }).ToList();
                return View(vm);
            }

            await _facade.CreateEvaluationAsync(profId, vm);
            return RedirectToAction(nameof(Index));
        }
    }
}
