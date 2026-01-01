using MentorEval.Models;
using MentorEval.Models.ViewModels;

namespace MentorEval.Services.Evaluations
{
    public interface IQuestionFactory
    {
        Question Create(QuestionCreateViewModel vm);
    }
}
