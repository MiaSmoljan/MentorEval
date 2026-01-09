using MentorEval.Models;
using Microsoft.EntityFrameworkCore;

namespace MentorEval.Services.Evaluations
{
    public class EvaluationQueryService : IEvaluationQueryService
    {
        private readonly AppDbContext _context;
        public EvaluationQueryService(AppDbContext context) => _context = context;

        public async Task<List<Evaluation>> GetForProfessorAsync(int professorId)
        {
            return await _context.Evaluations
                .Include(e => e.Course)
                .Where(e => e.Course.ProfessorId == professorId)
                .OrderByDescending(e => e.StartAt)
                .ToListAsync();
        }

        public int CountActive(List<Evaluation> evaluations, DateTime now)
        {
            return evaluations.Count(e => e.Status == "Active"
                                       && e.StartAt <= now
                                       && e.EndAt >= now);
        }
    }
}
