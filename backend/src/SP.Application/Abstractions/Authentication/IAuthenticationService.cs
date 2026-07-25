using SP.Domain.Abstractions;
using SP.Domain.Shared;
using SP.Domain.Users;

namespace SP.Application.Abstractions.Authentication;

public interface IAuthenticationService
{
    Task<Result<AuthenticationResult>> RegisterAsync(
        User user,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticationResult>> LoginAsync(
        Email email,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticationResult>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<Result> LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}
