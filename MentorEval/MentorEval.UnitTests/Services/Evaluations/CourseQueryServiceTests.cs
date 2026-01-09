using FluentAssertions;
using MentorEval.Models;
using MentorEval.Services.Evaluations;
using MentorEval.UnitTests.Infrastructure;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace MentorEval.UnitTests.Services.Evaluations;

public class CourseQueryServiceTests
{
    [Fact]
    public async Task GetProfessorCoursesAsync_returns_only_professors_courses_sorted_by_name()
    {
        using var h = DbTestHelper.CreateSqliteInMemoryDb();
        var db = h.Db;

        db.Users.AddRange(
            new User { Id = 1, Username = "p1", PasswordHash = "x", FullName = "Prof 1", Role = "Professor", Discriminator = "User" },
            new User { Id = 2, Username = "p2", PasswordHash = "x", FullName = "Prof 2", Role = "Professor", Discriminator = "User" }
        );

        db.Courses.AddRange(
            new Course { Name = "Z", Semester = 1, Year = 2025, ProfessorId = 1 },
            new Course { Name = "A", Semester = 1, Year = 2025, ProfessorId = 1 },
            new Course { Name = "B", Semester = 1, Year = 2025, ProfessorId = 2 }
        );

        await db.SaveChangesAsync();

        var svc = new CourseQueryService(db);

        var result = await svc.GetForProfessorAsync(1);

        result.Select(c => c.Name).Should().Equal("A", "Z");
        result.Should().OnlyContain(c => c.ProfessorId == 1);
    }
}
