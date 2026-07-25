using System.Security.Claims;

namespace SP.API.Auth;

public interface ICurrentUser
{
    Guid UserId { get; }
    string Email { get; }
    string Role { get; }
    bool IsAuthenticated { get; }
    ClaimsPrincipal Principal { get; }
}
