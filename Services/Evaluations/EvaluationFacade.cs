using MentorEval.Models;
using MentorEval.Models.ViewModels;

namespace MentorEval.Services.Evaluations
{
    public class EvaluationFacade
    {
        private readonly ICourseQueryService _courses;
        private readonly IEvaluationCreationService _creator;

        public EvaluationFacade(ICourseQueryService courses, IEvaluationCreationService creator)
        {
            _courses = courses;
            _creator = creator;
        }

        public Task<List<Course>> GetProfessorCoursesAsync(int professorId)
            => _courses.GetForProfessorAsync(professorId);

        public Task<int> CreateEvaluationAsync(int professorId, EvaluationCreateViewModel vm)
            => _creator.CreateAsync(professorId, vm);
    }
}
