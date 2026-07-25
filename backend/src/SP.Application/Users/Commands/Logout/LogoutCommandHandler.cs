using SP.Application.Abstractions.Authentication;
using SP.Application.Abstractions.Logging;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;

namespace SP.Application.Users.Commands.Logout;

public sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IUserContext _userContext;
    private readonly IAuditLogger _auditLogger;

    public LogoutCommandHandler(
        IAuthenticationService authenticationService,
        IUserContext userContext,
        IAuditLogger auditLogger)
    {
        _authenticationService = authenticationService;
        _userContext = userContext;
        _auditLogger = auditLogger;
    }

    public async Task<Result> HandleAsync(LogoutCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _authenticationService.LogoutAsync(command.RefreshToken, cancellationToken);
        if (result.IsSuccess)
        {
            _auditLogger.Log("UserLogout", _userContext.UserId);
        }
        return result;
    }
}
