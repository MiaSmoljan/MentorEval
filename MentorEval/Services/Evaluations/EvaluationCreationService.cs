using MentorEval.Models;
using Microsoft.EntityFrameworkCore;

namespace MentorEval.Services.Evaluations
{
    public class EvaluationCreationService : IEvaluationCreationService
    {
        private readonly AppDbContext _context;
        private readonly IQuestionFactory _questionFactory;

        public EvaluationCreationService(AppDbContext context, IQuestionFactory questionFactory)
        {
            _context = context;
            _questionFactory = questionFactory;
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

            foreach (var q in vm.Questions.Where(q => !string.IsNullOrWhiteSpace(q.Text)))
                eval.Questions.Add(_questionFactory.Create(q));

            _context.Evaluations.Add(eval);
            await _context.SaveChangesAsync();
            return eval.Id;
        }
    }
}
