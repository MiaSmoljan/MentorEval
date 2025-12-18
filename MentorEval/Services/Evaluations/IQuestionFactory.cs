using MentorEval.Models;

namespace MentorEval.Services.Evaluations
{
    public interface IQuestionFactory
    {
        Question Create(QuestionCreateViewModel vm);
    }
}
