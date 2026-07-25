using SP.Application.Abstractions.Authentication;
using SP.Application.Abstractions.Logging;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Shared;

namespace SP.Application.Users.Commands.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, AuthenticationResult>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuditLogger _auditLogger;

    public LoginCommandHandler(IAuthenticationService authenticationService, IAuditLogger auditLogger)
    {
        _authenticationService = authenticationService;
        _auditLogger = auditLogger;
    }

    public async Task<Result<AuthenticationResult>> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return Result.Failure<AuthenticationResult>(emailResult.Error);

        var result = await _authenticationService.LoginAsync(emailResult.Value, command.Password, cancellationToken);
        if (result.IsSuccess)
        {
            _auditLogger.Log("UserLogin", result.Value.User.Id, new { email = command.Email });
        }

        return result;
    }
}
