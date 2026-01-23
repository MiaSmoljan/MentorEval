using MentorEval.Models.ViewModels;

namespace MentorEval.Services.Evaluations.Validation
{
    public class Scale10ValidationStrategy : IQuestionValidationStrategy
    {
        public string Type => "Scale10";

        public void Validate(QuestionCreateViewModel q)
        {
            if (string.IsNullOrWhiteSpace(q.Text))
                throw new ArgumentException("Question text is required.");
        }
    }
}
