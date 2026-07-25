using System.Security.Claims;
using SP.Application.Abstractions.Authentication;

namespace SP.API.Auth;

internal sealed class CurrentUser : ICurrentUser, IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal Principal =>
        _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated ?? false;

    public Guid UserId
    {
        get
        {
            var value = Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public string Email => Principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    public string Role => Principal.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
}
