using MentorEval.Models;
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

        public EvaluationsController(AppDbContext context, IEvaluationCreationService creation)
        {
            _context = context;
            _creation = creation;
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

            var courses = await _context.Courses
                .Where(c => c.ProfessorId == profId)
                .ToListAsync();

            var vm = new EvaluationCreateViewModel
            {
                AvailableCourses = courses,
                StartAt = DateTime.Today,
                EndAt = DateTime.Today.AddDays(7),
                Questions = new List<QuestionCreateViewModel>
                {
                    new QuestionCreateViewModel { Text = "", Type = "Scale10", Required = true },
                    new QuestionCreateViewModel { Text = "", Type = "Text", Required = false }
                }
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
                vm.AvailableCourses = await _context.Courses
                    .Where(c => c.ProfessorId == profId)
                    .ToListAsync();
                return View(vm);
            }

            var eval = new Evaluation
            {
                Title = vm.Title,
                CourseId = vm.CourseId,
                StartAt = vm.StartAt,
                EndAt = vm.EndAt,
                Status = "Active"
            };

            foreach (var q in vm.Questions)
            {
                if (string.IsNullOrWhiteSpace(q.Text)) continue;

                eval.Questions.Add(new Question
                {
                    Text = q.Text,
                    Type = q.Type,
                    Required = q.Required
                });
            }

            _context.Evaluations.Add(eval);
            await _creation.CreateAsync(profId, vm);

            return RedirectToAction(nameof(Index));
        }
    }
}
