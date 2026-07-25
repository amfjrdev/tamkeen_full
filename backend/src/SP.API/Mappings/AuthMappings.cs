using SP.API.Contracts.Auth;
using SP.Application.Abstractions.Authentication;

namespace SP.API.Mappings;

public static class AuthMappings
{
    public static AuthenticationResponseDto ToDto(this AuthenticationResult result)
        => new()
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            UserId = result.User.Id,
            Email = result.User.Email,
            FirstName = result.User.FirstName,
            LastName = result.User.LastName,
            Role = result.User.Role.ToString()
        };
}
