using MentorEval.Models.ViewModels;

namespace MentorEval.Services.Evaluations.Validation
{
    public class TextValidationStrategy : IQuestionValidationStrategy
    {
        public string Type => "Text";

        public void Validate(QuestionCreateViewModel q)
        {
            if (string.IsNullOrWhiteSpace(q.Text))
                throw new ArgumentException("Question text is required.");

        }
    }
}
