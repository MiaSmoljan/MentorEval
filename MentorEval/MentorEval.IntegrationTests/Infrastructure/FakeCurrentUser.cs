using MentorEval.Services.Security;
using System.Security.Claims;

namespace MentorEval.IntegrationTests.Infrastructure;

public class FakeCurrentUser : ICurrentUser
{
    public int GetProfessorId(ClaimsPrincipal user)
        => 1;
}
