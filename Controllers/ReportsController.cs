using MentorEval.Models;
using MentorEval.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MentorEval.Controllers;

[Authorize(Roles = "Professor")]
public class ReportsController : Controller
{
    private readonly AppDbContext _context;
    private readonly PdfService _pdfService;
    private readonly ReportAnalyticsService _analytics;

    public ReportsController(AppDbContext context, PdfService pdfService, ReportAnalyticsService analytics)
    {
        _context = context;
        _pdfService = pdfService;
        _analytics = analytics;
    }

    private int GetCurrentProfessorId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(idStr);
    }

    [HttpGet]
    public async Task<IActionResult> EvaluationJson(int id)
    {
        int profId = GetCurrentProfessorId();
        var dto = await _analytics.BuildEvaluationReportAsync(id, profId);
        if (dto == null) return NotFound();
        return Json(dto);
    }

    [HttpGet]
    public async Task<IActionResult> EvaluationAnalyticsPdf(int id)
    {
        int profId = GetCurrentProfessorId();

        var report = await _analytics.BuildEvaluationReportAsync(id, profId);
        if (report == null) return NotFound();

        // ako je premalo odgovora, vrati poruku umjesto pdf-a
        if (!string.IsNullOrWhiteSpace(report.PrivacyNotice))
            return Content(report.PrivacyNotice);

        var pdfBytes = _pdfService.GenerateEvaluationAnalyticsReport(report);

        return File(pdfBytes, "application/pdf", $"evaluation_{id}_report.pdf");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Snapshot(int id)
    {
        int profId = GetCurrentProfessorId();
        var dto = await _analytics.BuildEvaluationReportAsync(id, profId);
        if (dto == null) return NotFound();

        await _analytics.SaveReportSnapshotAsync(id, dto);
        TempData["Msg"] = "Snapshot izvještaja spremljen.";
        return RedirectToAction("EvaluationJson", new { id });
    }
}
