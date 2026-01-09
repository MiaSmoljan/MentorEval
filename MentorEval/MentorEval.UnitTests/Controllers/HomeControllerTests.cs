using FluentAssertions;
using MentorEval.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Xunit;

namespace MentorEval.UnitTests.Controllers;

public class HomeControllerTests
{
    [Fact]
    public void Index_when_authenticated_redirects_to_ProfessorDashboard()
    {
        var sut = new HomeController();
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Role, "Professor"),
                }, authenticationType: "Test"))
            }
        };

        var res = sut.Index();
        res.Should().BeOfType<RedirectToActionResult>();

        var redirect = (RedirectToActionResult)res;
        redirect.ActionName.Should().Be("Dashboard");
    }

    [Fact]
    public void Index_when_anonymous_returns_View()
    {
        var sut = new HomeController();
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var res = sut.Index();
        res.Should().BeOfType<ViewResult>();
    }
}
