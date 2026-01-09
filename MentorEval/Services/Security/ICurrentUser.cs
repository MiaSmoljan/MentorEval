using System.Security.Claims;

namespace MentorEval.Services.Security
{
    public interface ICurrentUser
    {
        int GetProfessorId(ClaimsPrincipal user);
    }
}
