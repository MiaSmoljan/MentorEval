using MentorEval.Models.ViewModels;

namespace MentorEval.Services.Evaluations.Validation
{
    public class YesNoValidationStrategy : IQuestionValidationStrategy
    {
        public string Type => "YesNo";

        public void Validate(QuestionCreateViewModel q)
        {
            if (string.IsNullOrWhiteSpace(q.Text))
                throw new ArgumentException("Question text is required.");
        }
    }
}
