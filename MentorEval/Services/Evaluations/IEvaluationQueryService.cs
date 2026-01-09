using MentorEval.Models;

namespace MentorEval.Services.Evaluations
{
    public interface IEvaluationQueryService
    {
        Task<List<Evaluation>> GetForProfessorAsync(int professorId);
        int CountActive(List<Evaluation> evaluations, DateTime now);
    }
}
