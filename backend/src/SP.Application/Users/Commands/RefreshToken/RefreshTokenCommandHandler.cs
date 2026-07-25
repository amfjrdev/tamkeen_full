using SP.Application.Abstractions.Authentication;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;

namespace SP.Application.Users.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, AuthenticationResult>
{
    private readonly IAuthenticationService _authenticationService;

    public RefreshTokenCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<Result<AuthenticationResult>> HandleAsync(RefreshTokenCommand command, CancellationToken cancellationToken = default)
        => await _authenticationService.RefreshTokenAsync(command.RefreshToken, cancellationToken);
}
