using FluentAssertions;
using MentorEval.Controllers;
using MentorEval.Models;
using MentorEval.Models.ViewModels;
using MentorEval.Services.Evaluations;
using MentorEval.Services.Security;
using MentorEval.UnitTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace MentorEval.UnitTests.Controllers;

public class EvaluationsControllerTests
{
    private static EvaluationsController CreateController(
    Mock<IEvaluationQueryService> evalQuery,
    EvaluationFacade facade,
    int professorId = 1)
    {
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);
        currentUser
            .Setup(x => x.GetProfessorId(It.IsAny<ClaimsPrincipal>()))
            .Returns(professorId);

        var controller = new EvaluationsController(evalQuery.Object, facade, currentUser.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim(ClaimTypes.NameIdentifier, professorId.ToString()),
        new Claim(ClaimTypes.Role, "Professor")
    }, "Test"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        return controller;
    }


    [Fact]
    public async Task Create_GET_returns_view_with_courses_and_empty_questions()
    {
        var evalQuery = new Mock<IEvaluationQueryService>(MockBehavior.Strict);

        var courseQuery = new Mock<ICourseQueryService>(MockBehavior.Strict);
        var creation = new Mock<IEvaluationCreationService>(MockBehavior.Strict);

        courseQuery
            .Setup(s => s.GetForProfessorAsync(1))
            .ReturnsAsync(new List<Course>
            {
            new() { Id = 10, Name = "PI", ProfessorId = 1, Semester = 1, Year = 2025 }
            });

        var facade = new EvaluationFacade(courseQuery.Object, creation.Object);
        var controller = CreateController(evalQuery, facade);

        var result = await controller.Create();

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var vm = view.Model.Should().BeOfType<EvaluationCreateViewModel>().Subject;

        vm.AvailableCourses.Should().HaveCount(1);
        vm.AvailableCourses[0].Id.Should().Be(10);
        vm.Questions.Should().NotBeNull();

        courseQuery.VerifyAll();
        creation.VerifyNoOtherCalls();
        evalQuery.VerifyNoOtherCalls();
    }


    [Fact]
    public async Task Create_POST_invalid_model_returns_same_view_and_repopulates_courses()
    {
        using var h = DbTestHelper.CreateSqliteInMemoryDb();
        var db = h.Db;

        var courseQuery = new Mock<ICourseQueryService>(MockBehavior.Strict);
        var creation = new Mock<IEvaluationCreationService>(MockBehavior.Strict);
        courseQuery
            .Setup(s => s.GetForProfessorAsync(1))
            .ReturnsAsync(new List<Course>
            {
                new() { Id = 10, Name = "PI", ProfessorId = 1, Semester = 1, Year = 2025 }
            });

        var facade = new EvaluationFacade(courseQuery.Object, creation.Object);

        var evalQuery = new Mock<IEvaluationQueryService>(MockBehavior.Strict);
        var controller = CreateController(evalQuery, facade);
        controller.ModelState.AddModelError("Title", "Required");

        var vmIn = new EvaluationCreateViewModel { CourseId = 10, Title = "" };
        var result = await controller.Create(vmIn);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var vmOut = view.Model.Should().BeOfType<EvaluationCreateViewModel>().Subject;
        vmOut.AvailableCourses.Should().HaveCount(1);
        vmOut.AvailableCourses[0].Id.Should().Be(10);

        courseQuery.VerifyAll();
        creation.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Create_POST_valid_model_calls_facade_and_redirects_to_Index()
    {
        using var h = DbTestHelper.CreateSqliteInMemoryDb();
        var db = h.Db;

        var courseQuery = new Mock<ICourseQueryService>(MockBehavior.Strict);
        var creation = new Mock<IEvaluationCreationService>(MockBehavior.Strict);
        creation
            .Setup(s => s.CreateAsync(1, It.IsAny<EvaluationCreateViewModel>()))
            .ReturnsAsync(123);

        var facade = new EvaluationFacade(courseQuery.Object, creation.Object);

        var evalQuery = new Mock<IEvaluationQueryService>(MockBehavior.Strict);
        var controller = CreateController(evalQuery, facade);

        var vm = new EvaluationCreateViewModel
        {
            CourseId = 10,
            Title = "Eval",
            StartAt = DateTime.Today,
            EndAt = DateTime.Today.AddDays(1),
            Questions = new List<QuestionCreateViewModel>
            {
                new() { Text = "Komentar?", Type = "Text", Required = true }
            }
        };

        var result = await controller.Create(vm);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");

        creation.VerifyAll();
        courseQuery.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Index_returns_evaluations_from_query_and_sets_active_count()
    {
        var profId = 7;

        var evaluations = new List<Evaluation>
    {
        new() { Title = "A", Status = "Active", StartAt = DateTime.Today.AddDays(-1), EndAt = DateTime.Today.AddDays(1) },
        new() { Title = "B", Status = "Inactive", StartAt = DateTime.Today.AddDays(-10), EndAt = DateTime.Today.AddDays(-5) }
    };

        var evalQuery = new Mock<IEvaluationQueryService>(MockBehavior.Strict);
        evalQuery
            .Setup(q => q.GetForProfessorAsync(profId))
            .ReturnsAsync(evaluations);

        evalQuery
            .Setup(q => q.CountActive(evaluations, It.IsAny<DateTime>()))
            .Returns(1);

        var courseQuery = new Mock<ICourseQueryService>(MockBehavior.Strict);
        var creation = new Mock<IEvaluationCreationService>(MockBehavior.Strict);
        var facade = new EvaluationFacade(courseQuery.Object, creation.Object);

        var controller = CreateController(evalQuery, facade, professorId: profId);

        var result = await controller.Index();

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var list = view.Model.Should().BeAssignableTo<List<Evaluation>>().Subject;

        list.Should().BeSameAs(evaluations);
        ((int?)controller.ViewBag.ActiveEvaluationsCount).Should().Be(1);

        evalQuery.VerifyAll();
        courseQuery.VerifyNoOtherCalls();
        creation.VerifyNoOtherCalls();
    }

}
