using MentorEval.Models;

namespace MentorEval.Services.Evaluations
{
    public interface IEvaluationCreationService
    {
        Task<int> CreateAsync(int professorId, EvaluationCreateViewModel vm);
    }
}
