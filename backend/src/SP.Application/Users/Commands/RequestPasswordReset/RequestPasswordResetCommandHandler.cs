using SP.Application.Abstractions;
using SP.Application.Abstractions.Messaging;
using SP.Application.Abstractions.Authentication;
using SP.Domain.Abstractions;
using SP.Domain.Users;

namespace SP.Application.Users.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetCommandHandler : ICommandHandler<RequestPasswordResetCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IAppSettings _appSettings;

    public RequestPasswordResetCommandHandler(
        IUserRepository userRepository,
        IEmailService emailService,
        IAppSettings appSettings)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _appSettings = appSettings;
    }

    public async Task<Result> HandleAsync(RequestPasswordResetCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (user is null)
            return Result.Success(); // Don't reveal if email exists

        // Generate reset token (simplified - in real implementation would use secure token generation)
        var resetToken = Guid.NewGuid().ToString();
        
        // In real implementation, store token with expiration in database
        // For now, we'll just send the email
        
        await _emailService.SendPasswordResetEmailAsync(
            Domain.Shared.Email.Create(user.Email).Value,
            $"{user.FirstName} {user.LastName}",
            $"{_appSettings.BaseUrl}/reset-password?token={resetToken}",
            cancellationToken);

        return Result.Success();
    }
}