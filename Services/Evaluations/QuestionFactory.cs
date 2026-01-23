using MentorEval.Models;
using MentorEval.Models.ViewModels;

namespace MentorEval.Services.Evaluations
{
    public class QuestionFactory : IQuestionFactory
    {
        private readonly IReadOnlyDictionary<string, IQuestionHandler> _handlers;

        public QuestionFactory(IEnumerable<IQuestionHandler> handlers)
            => _handlers = handlers.ToDictionary(h => h.Type, h => h);

        public Question Create(QuestionCreateViewModel vm)
        {
            if (!_handlers.TryGetValue(vm.Type, out var handler))
                throw new NotSupportedException($"Unsupported question type: {vm.Type}");

            return handler.Build(vm);
        }
    }
}
