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
        return int.Parse(idStr);
    }

    // LISTA evaluacija koje student može ispuniti
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        int studentId = GetStudentId();
        var now = DateTime.Now;

        // Ako imate assignment (EvaluationStudent), prikazat će se samo dodijeljene.
        // Ako NEMA dodijeljenih (0 students), evaluacija je "open for all".
        var evaluations = await _db.Evaluations
            .Include(e => e.Course)
            .Include(e => e.Students)
            .Where(e =>
                e.Status != "Draft" &&
                e.StartAt <= now && now <= e.EndAt &&
                (e.Students.Any(s => s.Id == studentId) || !e.Students.Any()))
            .OrderByDescending(e => e.StartAt)
            .ToListAsync();

        // da označimo koje su već riješene
        var answeredEvalIds = await _db.Answers
            .Where(a => a.StudentId == studentId)
            .Select(a => a.EvaluationId)
            .Distinct()
            .ToListAsync();

        ViewBag.AnsweredEvalIds = answeredEvalIds;

        return View(evaluations);
    }

    // PRIKAZ forme
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

        // dostupnost
        if (eval.Status == "Draft" || now < eval.StartAt || now > eval.EndAt)
            return Forbid();

        // assignment (ako postoji)
        if (eval.Students.Any() && !eval.Students.Any(s => s.Id == studentId))
            return Forbid();

        // zabrani duplo ispunjavanje
        bool alreadyAnswered = await _db.Answers.AnyAsync(a => a.EvaluationId == id && a.StudentId == studentId);
        if (alreadyAnswered)
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

    // SUBMIT forme
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

        if (eval.Status == "Draft" || now < eval.StartAt || now > eval.EndAt)
            return Forbid();

        if (eval.Students.Any() && !eval.Students.Any(s => s.Id == studentId))
            return Forbid();

        bool alreadyAnswered = await _db.Answers.AnyAsync(a => a.EvaluationId == eval.Id && a.StudentId == studentId);
        if (alreadyAnswered)
        {
            TempData["Msg"] = "Već ste ispunili ovu evaluaciju.";
            return RedirectToAction(nameof(Index));
        }

        // VALIDACIJA required polja
        foreach (var q in vm.Questions)
        {
            if (!q.Required) continue;

            if (q.Type == "Scale10" && !q.Grade.HasValue)
                ModelState.AddModelError("", $"Obavezno pitanje: \"{q.Text}\" (ocjena 1-10).");

            if (q.Type == "Text" && string.IsNullOrWhiteSpace(q.AnswerText))
                ModelState.AddModelError("", $"Obavezno pitanje: \"{q.Text}\" (komentar).");
        }

        if (!ModelState.IsValid)
        {
            // popuni title/course za view ako se vraćamo na formu
            vm.Title = eval.Title;
            vm.CourseName = (await _db.Courses.Where(c => c.Id == eval.CourseId).Select(c => c.Name).FirstAsync());
            vm.StartAt = eval.StartAt;
            vm.EndAt = eval.EndAt;
            return View(vm);
        }

        // SPREMANJE answer-a
        foreach (var q in vm.Questions)
        {
            // opcionalna pitanja: ako ništa nije uneseno, preskoči
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
