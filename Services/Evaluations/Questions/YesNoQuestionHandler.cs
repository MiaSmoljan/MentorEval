using MentorEval.Models;
using MentorEval.Models.ViewModels;

namespace MentorEval.Services.Evaluations.Questions
{
    public class YesNoQuestionHandler : IQuestionHandler
    {
        public string Type => "YesNo";

        public Question Build(QuestionCreateViewModel vm) =>
            new Question { Text = vm.Text, Type = vm.Type, Required = vm.Required };
    }
}
