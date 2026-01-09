using FluentAssertions;
using MentorEval.Models;
using MentorEval.Models.ViewModels;
using MentorEval.Services.Evaluations;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MentorEval.UnitTests.Services
{
    public class EvaluationFacadeTests
    {
        [Fact]
        public async Task GetProfessorCoursesAsync_calls_course_service_and_returns_result()
        {
            var courses = new Mock<ICourseQueryService>();
            var creator = new Mock<IEvaluationCreationService>();

            courses.Setup(s => s.GetForProfessorAsync(5))
                   .ReturnsAsync(new List<Course>
                   {
                   new() { Id = 10, Name = "PI", ProfessorId = 5 }
                   });

            var facade = new EvaluationFacade(courses.Object, creator.Object);

            var result = await facade.GetProfessorCoursesAsync(5);

            result.Should().HaveCount(1);
            result[0].Name.Should().Be("PI");

            courses.Verify(s => s.GetForProfessorAsync(5), Times.Once);
            creator.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateEvaluationAsync_calls_creator_and_returns_id()
        {
            var courses = new Mock<ICourseQueryService>();
            var creator = new Mock<IEvaluationCreationService>();

            var vm = new EvaluationCreateViewModel { Title = "Eval", CourseId = 10 };
            creator.Setup(s => s.CreateAsync(1, vm)).ReturnsAsync(123);

            var facade = new EvaluationFacade(courses.Object, creator.Object);

            var id = await facade.CreateEvaluationAsync(1, vm);

            id.Should().Be(123);
            creator.Verify(s => s.CreateAsync(1, vm), Times.Once);
            courses.VerifyNoOtherCalls();
        }
    }
}
