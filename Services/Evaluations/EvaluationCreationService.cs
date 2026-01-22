using MentorEval.Models;
using MentorEval.Models.ViewModels;
using MentorEval.Services.Evaluations.Validation;
using Microsoft.EntityFrameworkCore;

namespace MentorEval.Services.Evaluations
{
    public class EvaluationCreationService : IEvaluationCreationService
    {
        private readonly AppDbContext _context;
        private readonly IQuestionFactory _factory;
        private readonly QuestionValidationResolver _validator;

        public EvaluationCreationService(
            AppDbContext context,
            IQuestionFactory factory,
            QuestionValidationResolver validator)
        {
            _context = context;
            _factory = factory;
            _validator = validator;
        }

        public async Task<int> CreateAsync(int professorId, EvaluationCreateViewModel vm)
        {
            bool courseOk = await _context.Courses.AnyAsync(c => c.Id == vm.CourseId && c.ProfessorId == professorId);
            if (!courseOk) throw new InvalidOperationException("Course does not belong to current professor.");

            var eval = new Evaluation
            {
                Title = vm.Title,
                CourseId = vm.CourseId,
                StartAt = vm.StartAt,
                EndAt = vm.EndAt,
                Status = "Active"
            };

            foreach (var qVm in vm.Questions.Where(q => !string.IsNullOrWhiteSpace(q.Text)))
            {
                _validator.Validate(qVm);
                eval.Questions.Add(_factory.Create(qVm));
            }

            _context.Evaluations.Add(eval);
            await _context.SaveChangesAsync();
            return eval.Id;
        }
    }
}
