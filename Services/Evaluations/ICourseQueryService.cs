using MentorEval.Models;

namespace MentorEval.Services.Evaluations
{
    public interface ICourseQueryService
    {
        Task<List<Course>> GetForProfessorAsync(int professorId);
    }
}
