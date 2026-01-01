using MentorEval.Models;
using Microsoft.EntityFrameworkCore;

namespace MentorEval.Services.Evaluations
{
    public class CourseQueryService : ICourseQueryService
    {
        private readonly AppDbContext _context;
        public CourseQueryService(AppDbContext context) => _context = context;

        public Task<List<Course>> GetForProfessorAsync(int professorId)
        {
            return _context.Courses
                .Where(c => c.ProfessorId == professorId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
