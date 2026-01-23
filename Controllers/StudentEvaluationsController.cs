using MentorEval.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MentorEval.Controllers;

[Authorize(Roles = "Student")]
public class StudentEvaluationsController : Controller
{
    private readonly AppDbContext _db;

    public StudentEvaluationsController(AppDbContext db) => _db = db;

    private int GetStudentId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(idStr, out var id))
            throw new UnauthorizedAccessException("Invalid or missing user id claim.");

        return id;
    }

    private static bool IsEvaluationOpen(Evaluation eval, DateTime now)
        => eval.Status != "Draft" && eval.StartAt <= now && now <= eval.EndAt;

    private static bool StudentHasAccess(Evaluation eval, int studentId)
        => eval.Students.Count == 0 || eval.Students.Any(s => s.Id == studentId);

    private async Task<bool> AlreadyAnsweredAsync(int evaluationId, int studentId)
        => await _db.Answers.AnyAsync(a => a.EvaluationId == evaluationId && a.StudentId == studentId);

    private void ValidateRequiredAnswers(StudentEvaluationFillViewModel vm)
    {
        foreach (var q in vm.Questions)
        {
            if (!q.Required) continue;

            if (q.Type == "Scale10" && !q.Grade.HasValue)
                ModelState.AddModelError("", $"Obavezno pitanje: \"{q.Text}\" (ocjena 1-10).");

            if (q.Type == "Text" && string.IsNullOrWhiteSpace(q.AnswerText))
                ModelState.AddModelError("", $"Obavezno pitanje: \"{q.Text}\" (komentar).");
        }
    }
    private static ForbidResult? GuardEvaluation(Evaluation eval, int studentId, DateTime now)
    {
        if (!IsEvaluationOpen(eval, now)) return new ForbidResult();
        if (!StudentHasAccess(eval, studentId)) return new ForbidResult();
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        int studentId = GetStudentId();
        var now = DateTime.Now;

        var evaluations = await _db.Evaluations
            .Include(e => e.Course)
            .Include(e => e.Students)
            .Where(e =>
                e.Status != "Draft" &&
                e.StartAt <= now && now <= e.EndAt &&
                (e.Students.Any(s => s.Id == studentId) || e.Students.Count == 0))
            .OrderByDescending(e => e.StartAt)
            .ToListAsync();

        var answeredEvalIds = await _db.Answers
            .Where(a => a.StudentId == studentId)
            .Select(a => a.EvaluationId)
            .Distinct()
            .ToListAsync();

        ViewBag.AnsweredEvalIds = answeredEvalIds;

        return View(evaluations);
    }

    [HttpGet]
    public async Task<IActionResult> Fill(int id)
    {
        int studentId = GetStudentId();
        var now = DateTime.Now;

        var eval = await _db.Evaluations
            .Include(e => e.Course)
            .Include(e => e.Students)
            .Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eval == null) return NotFound();

        var guard = GuardEvaluation(eval, studentId, now);
        if (guard != null) return guard;

        if (await AlreadyAnsweredAsync(id, studentId))
        {
            TempData["Msg"] = "Već ste ispunili ovu evaluaciju.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new StudentEvaluationFillViewModel
        {
            EvaluationId = eval.Id,
            Title = eval.Title,
            CourseName = eval.Course?.Name ?? "",
            StartAt = eval.StartAt,
            EndAt = eval.EndAt,
            Questions = eval.Questions
                .OrderBy(q => q.Id)
                .Select(q => new StudentQuestionAnswerVM
                {
                    QuestionId = q.Id,
                    Text = q.Text,
                    Type = q.Type,
                    Required = q.Required
                })
                .ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Fill(StudentEvaluationFillViewModel vm)
    {
        int studentId = GetStudentId();
        var now = DateTime.Now;

        var eval = await _db.Evaluations
            .Include(e => e.Students)
            .Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Id == vm.EvaluationId);

        if (eval == null) return NotFound();

        var guard = GuardEvaluation(eval, studentId, now);
        if (guard != null) return guard;

        if (await AlreadyAnsweredAsync(eval.Id, studentId))
        {
            TempData["Msg"] = "Već ste ispunili ovu evaluaciju.";
            return RedirectToAction(nameof(Index));
        }

        ValidateRequiredAnswers(vm);

        if (!ModelState.IsValid)
        {
            vm.Title = eval.Title;

            vm.CourseName = await _db.Courses
                .Where(c => c.Id == eval.CourseId)
                .Select(c => c.Name)
                .FirstAsync();

            vm.StartAt = eval.StartAt;
            vm.EndAt = eval.EndAt;

            return View(vm);
        }

        foreach (var q in vm.Questions)
        {
            bool emptyScale = q.Type == "Scale10" && !q.Grade.HasValue;
            bool emptyText = q.Type == "Text" && string.IsNullOrWhiteSpace(q.AnswerText);

            if (!q.Required && (emptyScale || emptyText))
                continue;

            _db.Answers.Add(new Answer
            {
                EvaluationId = eval.Id,
                QuestionId = q.QuestionId,
                StudentId = studentId,
                SubmittedAt = DateTime.Now,
                Grade = q.Type == "Scale10" ? q.Grade : null,
                Text = q.Type == "Text" ? q.AnswerText?.Trim() : null
            });
        }

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(ThankYou), new { id = eval.Id });
    }

    [HttpGet]
    public IActionResult ThankYou(int id)
    {
        ViewBag.EvaluationId = id;
        return View();
    }
}
