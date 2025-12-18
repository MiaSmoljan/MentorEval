using MentorEval.Models;

namespace MentorEval.Services.Evaluations
{
    public interface IQuestionHandler
    {
        string Type { get; }
        Question Build(QuestionCreateViewModel vm);
    }
}
