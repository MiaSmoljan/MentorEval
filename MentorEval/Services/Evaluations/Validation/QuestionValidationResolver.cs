using MentorEval.Models.ViewModels;

namespace MentorEval.Services.Evaluations.Validation
{
    public class QuestionValidationResolver
    {
        private readonly IReadOnlyDictionary<string, IQuestionValidationStrategy> _map;

        public QuestionValidationResolver(IEnumerable<IQuestionValidationStrategy> strategies)
            => _map = strategies.ToDictionary(s => s.Type, s => s);

        public void Validate(QuestionCreateViewModel q)
        {
            if (!_map.TryGetValue(q.Type, out var strategy))
                throw new NotSupportedException($"Unsupported question type: {q.Type}");

            strategy.Validate(q);
        }
    }
}
