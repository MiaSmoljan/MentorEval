using MentorEval.Models;
using MentorEval.Models.ViewModels;
using MentorEval.Services.Evaluations;
using MentorEval.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MentorEval.Controllers
{
    [Authorize(Roles = "Professor")]
    public class EvaluationsController : Controller
    {
        private readonly IEvaluationQueryService _evalQuery;
        private readonly EvaluationFacade _facade;
        private readonly ICurrentUser _currentUser;

        public EvaluationsController(IEvaluationQueryService evalQuery, EvaluationFacade facade, ICurrentUser currentUser)
        {
            _evalQuery = evalQuery;
            _facade = facade;
            _currentUser = currentUser;
        }

        private int GetCurrentProfessorId() => _currentUser.GetProfessorId(User);


        public async Task<IActionResult> Index()
        {
            int profId = GetCurrentProfessorId();

            var evaluations = await _evalQuery.GetForProfessorAsync(profId);

            var now = DateTime.Now;
            ViewBag.ActiveEvaluationsCount = _evalQuery.CountActive(evaluations, now);

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
