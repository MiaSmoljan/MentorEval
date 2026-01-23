using MentorEval.Models.ViewModels;

namespace MentorEval.Services.Evaluations.Validation
{
    public interface IQuestionValidationStrategy
    {
        string Type { get; }
        void Validate(QuestionCreateViewModel q);
    }
}
