
using SP.Domain.Shared;

namespace SP.Application.Abstractions.Authentication;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(Email email, string userName, string resetLink, CancellationToken cancellationToken = default);
    Task SendVerificationEmailAsync(Email email, string userName, string verificationLink, CancellationToken cancellationToken = default);
    Task SendEmailAsync(string recipientEmail, string subject, string body, CancellationToken cancellationToken = default);
}
