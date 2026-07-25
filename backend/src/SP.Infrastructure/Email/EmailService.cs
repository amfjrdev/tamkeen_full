using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SP.Application.Abstractions.Authentication;
using DomainEmail = SP.Domain.Shared.Email;

namespace SP.Infrastructure.Email;

internal sealed class EmailService : IEmailService
{
    private readonly SmtpOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _options = configuration.GetSection("Smtp").Get<SmtpOptions>() ?? new SmtpOptions();
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(
        DomainEmail email, string userName, string resetLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "Reset your password";
        var body = $"""
            <h2>Hello {WebUtility.HtmlEncode(userName)},</h2>
            <p>Click the link below to reset your password. This link expires in 1 hour.</p>
            <p><a href="{WebUtility.HtmlEncode(resetLink)}">Reset Password</a></p>
            <p>If you did not request this, ignore this email.</p>
            """;
        return SendEmailAsync(email.Value, subject, body, cancellationToken);
    }

    public Task SendVerificationEmailAsync(
        DomainEmail email, string userName, string verificationLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "Verify your email address";
        var body = $"""
            <h2>Welcome {WebUtility.HtmlEncode(userName)}!</h2>
            <p>Please verify your email address by clicking the link below.</p>
            <p><a href="{WebUtility.HtmlEncode(verificationLink)}">Verify Email</a></p>
            """;
        return SendEmailAsync(email.Value, subject, body, cancellationToken);
    }

    public async Task SendEmailAsync(
        string recipientEmail, string subject, string body,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Email (disabled) → {To}: {Subject}", recipientEmail, subject);
            return;
        }

        try
        {
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = new NetworkCredential(_options.Username, _options.Password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromAddress, _options.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(recipientEmail);

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent → {To}: {Subject}", recipientEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", recipientEmail);
        }
    }
}

internal sealed class SmtpOptions
{
    public bool Enabled { get; init; } = false;
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public bool EnableSsl { get; init; } = true;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromAddress { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
}
