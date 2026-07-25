using SP.Domain.Users;

namespace SP.Application.Abstractions.Authentication;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
