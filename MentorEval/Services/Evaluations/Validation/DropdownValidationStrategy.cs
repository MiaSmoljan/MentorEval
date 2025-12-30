using MentorEval.Models.ViewModels;

namespace MentorEval.Services.Evaluations.Validation
{
    public class DropdownValidationStrategy : IQuestionValidationStrategy
    {
        public string Type => "Dropdown";

        public void Validate(QuestionCreateViewModel q)
        {
            if (string.IsNullOrWhiteSpace(q.Text))
                throw new ArgumentException("Question text is required.");

        }
    }
}
