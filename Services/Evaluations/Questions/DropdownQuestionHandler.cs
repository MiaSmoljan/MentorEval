using MentorEval.Models;
using MentorEval.Models.ViewModels;

namespace MentorEval.Services.Evaluations.Questions
{
    public class DropdownQuestionHandler : IQuestionHandler
    {
        public string Type => "Dropdown";

        public Question Build(QuestionCreateViewModel vm) =>
            new Question { Text = vm.Text, Type = vm.Type, Required = vm.Required };
    }
}
