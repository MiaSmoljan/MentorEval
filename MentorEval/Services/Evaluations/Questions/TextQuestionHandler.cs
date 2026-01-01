using MentorEval.Models;
using MentorEval.Models.ViewModels;

namespace MentorEval.Services.Evaluations.Questions
{
    public class TextQuestionHandler : IQuestionHandler
    {
        public string Type => "Text";

        public Question Build(QuestionCreateViewModel vm) =>
            new Question { Text = vm.Text, Type = vm.Type, Required = vm.Required };
    }
}
