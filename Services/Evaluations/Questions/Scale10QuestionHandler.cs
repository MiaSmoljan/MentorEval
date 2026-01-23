using MentorEval.Models;
using MentorEval.Models.ViewModels;

namespace MentorEval.Services.Evaluations.Questions
{
    public class Scale10QuestionHandler : IQuestionHandler
    {
        public string Type => "Scale10";

        public Question Build(QuestionCreateViewModel vm) =>
            new Question { Text = vm.Text, Type = vm.Type, Required = vm.Required };
    }
}
