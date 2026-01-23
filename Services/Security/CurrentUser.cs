using System.Security.Claims;

namespace MentorEval.Services.Security
{
    public class CurrentUser : ICurrentUser
    {
        public int GetProfessorId(ClaimsPrincipal user)
        {
            var idStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(idStr!);
        }
    }
}
