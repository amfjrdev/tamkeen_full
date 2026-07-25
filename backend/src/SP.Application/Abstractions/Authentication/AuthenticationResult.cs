using SP.Domain.Users;

namespace SP.Application.Abstractions.Authentication;

public record AuthenticationResult(
    string AccessToken,
    string RefreshToken,
    User User);
