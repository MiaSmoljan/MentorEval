using MentorEval.Models;
using MentorEval.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace MentorEval.Controllers
{
    [ExcludeFromCodeCoverage]
    [Authorize(Roles = "Professor")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PdfService _pdfService;

        public ReportsController(AppDbContext context, PdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        private int GetCurrentProfessorId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(idStr);
        }

        [HttpGet]
        public async Task<IActionResult> ProfessorEvaluations()
        {
            int profId = GetCurrentProfessorId();

            var evaluations = await _context.Evaluations
                .Include(e => e.Course)
                .Where(e => e.Course.ProfessorId == profId)
                .OrderByDescending(e => e.StartAt)
                .ToListAsync();

            var pdfBytes = _pdfService.GenerateProfessorEvaluationsReport(evaluations);

            var fileName = $"evaluations-prof-{profId}-{DateTime.Now:yyyyMMddHHmm}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
