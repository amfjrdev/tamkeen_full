using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SP.Application.Abstractions.Authentication;

namespace SP.Infrastructure.Authentication;

internal sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public Guid UserId
    {
        get
        {
            var value = Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public string Email => Principal?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    public string Role => Principal?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
}
