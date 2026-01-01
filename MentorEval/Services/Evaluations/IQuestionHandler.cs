using MentorEval.Models;
using MentorEval.Models.ViewModels;

namespace MentorEval.Services.Evaluations
{
    public interface IQuestionHandler
    {
        string Type { get; }
        Question Build(QuestionCreateViewModel vm);
    }
}
