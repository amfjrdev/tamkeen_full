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
        if (string.IsNullOrWhiteSpace(command.Email))
            return Result.Success();

        var user = await _userRepository.GetByEmailAsync(command.Email.Trim(), cancellationToken);
        if (user is null)
            return Result.Success(); // Don't reveal if email exists

        var resetToken = user.Email;
        var baseUrl = _appSettings.BaseUrl?.TrimEnd('/') ?? "https://api.tamkeendz.com";
        var resetLink = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(fullName))
            fullName = user.Email;

        await _emailService.SendPasswordResetEmailAsync(
            Domain.Shared.Email.Create(user.Email).Value,
            fullName,
            resetLink,
            cancellationToken);

        return Result.Success();
    }
}