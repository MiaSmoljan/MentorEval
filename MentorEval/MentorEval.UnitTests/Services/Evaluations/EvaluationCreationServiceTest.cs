using FluentAssertions;
using MentorEval.Models;
using MentorEval.Models.ViewModels;
using MentorEval.Services.Evaluations;
using MentorEval.Services.Evaluations.Validation;
using MentorEval.UnitTests.Infrastructure;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MentorEval.UnitTests.Services.Evaluations;

public class EvaluationCreationServiceTests
{
    [Fact]
    public async Task CreateAsync_throws_when_course_does_not_belong_to_professor()
    {
        using var h = DbTestHelper.CreateSqliteInMemoryDb();
        var db = h.Db;

        db.Users.Add(new User
        {
            Username = "prof2",
            PasswordHash = "x",
            FullName = "P2",
            Role = "Professor",
            Discriminator = "User"
        });
        await db.SaveChangesAsync();
        var realProfId = db.Users.Single().Id;

        db.Courses.Add(new Course { Name = "PI", ProfessorId = realProfId, Semester = 1, Year = 2025 });
        await db.SaveChangesAsync();

        var questionFactory = new Mock<IQuestionFactory>(MockBehavior.Strict);

        var resolver = new QuestionValidationResolver(new IQuestionValidationStrategy[]
        {
            new TextValidationStrategy(),
            new YesNoValidationStrategy(),
        });

        var sut = new EvaluationCreationService(db, questionFactory.Object, resolver);

        var vm = new EvaluationCreateViewModel
        {
            CourseId = db.Courses.Single().Id,
            Title = "Eval",
            StartAt = DateTime.Today,
            EndAt = DateTime.Today.AddDays(1),
            Questions = new List<QuestionCreateViewModel>
            {
                new() { Text = "Komentar?", Type = "Text", Required = true }
            }
        };

        var act = async () => await sut.CreateAsync(professorId: 999, vm);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Course does not belong to current professor*");
    }

    [Fact]
    public async Task CreateAsync_creates_evaluation_and_questions_when_valid()
    {
        using var h = DbTestHelper.CreateSqliteInMemoryDb();
        var db = h.Db;

        db.Users.Add(new User { Username = "prof", PasswordHash = "x", FullName = "Prof", Role = "Professor", Discriminator = "User" });
        await db.SaveChangesAsync();
        var profId = db.Users.Single().Id;

        db.Courses.Add(new Course { Name = "PI", ProfessorId = profId, Semester = 1, Year = 2025 });
        await db.SaveChangesAsync();
        var courseId = db.Courses.Single().Id;

        var questionFactory = new Mock<IQuestionFactory>(MockBehavior.Strict);
        questionFactory
            .Setup(f => f.Create(It.IsAny<QuestionCreateViewModel>()))
            .Returns((QuestionCreateViewModel q) => new Question
            {
                Text = q.Text,
                Type = q.Type,
                Required = q.Required
            });

        var resolver = new QuestionValidationResolver(new IQuestionValidationStrategy[]
        {
            new TextValidationStrategy(),
            new YesNoValidationStrategy(),
        });

        var sut = new EvaluationCreationService(db, questionFactory.Object, resolver);

        var vm = new EvaluationCreateViewModel
        {
            CourseId = courseId,
            Title = "Eval Web",
            StartAt = DateTime.Today,
            EndAt = DateTime.Today.AddDays(3),
            Questions = new List<QuestionCreateViewModel>
            {
                new() { Text = "Komentar?", Type = "Text", Required = true },
                new() { Text = "Preporučuješ?", Type = "YesNo", Required = false }
            }
        };

        await sut.CreateAsync(profId, vm);

        var eval = db.Evaluations.Single();
        eval.CourseId.Should().Be(courseId);
        eval.Title.Should().Be("Eval Web");
        eval.Status.Should().Be("Active");

        var questions = db.Questions.OrderBy(q => q.Id).ToList();
        questions.Should().HaveCount(2);
        questions.All(q => q.EvaluationId == eval.Id).Should().BeTrue();

        questionFactory.Verify(f => f.Create(It.IsAny<QuestionCreateViewModel>()), Times.Exactly(2));
    }
}
