using System.Security.Claims;

namespace MentorEval.Services.Security
{
    public class CurrentUser : ICurrentUser
    {
        public int GetProfessorId(ClaimsPrincipal user)
            => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
