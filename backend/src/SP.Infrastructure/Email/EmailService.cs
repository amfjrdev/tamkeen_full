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
        _logger = logger;
        _options = LoadSmtpOptions(configuration);

        _logger.LogInformation(
            "EmailService initialized. Enabled: {Enabled}, Host: {Host}, Port: {Port}, User: {User}, From: {From}",
            _options.Enabled,
            _options.Host,
            _options.Port,
            _options.Username,
            _options.FromAddress);
    }

    private static SmtpOptions LoadSmtpOptions(IConfiguration configuration)
    {
        var options = configuration.GetSection("Smtp").Get<SmtpOptions>() ?? new SmtpOptions();

        // Check flat environment variables as fallback
        var envHost = configuration["SMTP_HOST"] ?? configuration["Smtp:Host"];
        if (!string.IsNullOrWhiteSpace(envHost) && (string.IsNullOrWhiteSpace(options.Host) || options.Host == "<set-via-env>"))
            options.Host = envHost;

        var envPortStr = configuration["SMTP_PORT"] ?? configuration["Smtp:Port"];
        if (int.TryParse(envPortStr, out var envPort))
            options.Port = envPort;

        var envUser = configuration["SMTP_USERNAME"] ?? configuration["Smtp:Username"];
        if (!string.IsNullOrWhiteSpace(envUser) && (string.IsNullOrWhiteSpace(options.Username) || options.Username == "<set-via-env>"))
            options.Username = envUser;

        var envPass = configuration["SMTP_PASSWORD"] ?? configuration["Smtp:Password"];
        if (!string.IsNullOrWhiteSpace(envPass) && (string.IsNullOrWhiteSpace(options.Password) || options.Password == "<set-via-env>"))
            options.Password = envPass;

        var envFrom = configuration["SMTP_FROM"] ?? configuration["Smtp:FromAddress"];
        if (!string.IsNullOrWhiteSpace(envFrom) && (string.IsNullOrWhiteSpace(options.FromAddress) || options.FromAddress == "<set-via-env>"))
            options.FromAddress = envFrom;
        else if (string.IsNullOrWhiteSpace(options.FromAddress) || options.FromAddress == "<set-via-env>")
            options.FromAddress = options.Username;

        var envFromName = configuration["SMTP_FROM_NAME"] ?? configuration["Smtp:FromName"];
        if (!string.IsNullOrWhiteSpace(envFromName) && (string.IsNullOrWhiteSpace(options.FromName) || options.FromName == "<set-via-env>"))
            options.FromName = envFromName;
        else if (string.IsNullOrWhiteSpace(options.FromName) || options.FromName == "<set-via-env>")
            options.FromName = "Tamkeen";

        var envEnabledStr = configuration["SMTP_ENABLED"] ?? configuration["Smtp:Enabled"];
        if (bool.TryParse(envEnabledStr, out var envEnabled))
        {
            options.Enabled = envEnabled;
        }
        else if (!string.IsNullOrWhiteSpace(options.Host) && options.Host != "<set-via-env>" &&
                 !string.IsNullOrWhiteSpace(options.Username) && options.Username != "<set-via-env>")
        {
            options.Enabled = true;
        }

        return options;
    }

    public Task SendPasswordResetEmailAsync(
        DomainEmail email, string userName, string resetLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "إعادة تعيين كلمة المرور - تمكين";
        var safeUserName = WebUtility.HtmlEncode(userName?.Trim() ?? "المستخدم");
        var safeResetLink = WebUtility.HtmlEncode(resetLink);

        var body = $$"""
            <!DOCTYPE html>
            <html lang="ar" dir="rtl">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>إعادة تعيين كلمة المرور</title>
                <style>
                    body {
                        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                        background-color: #f1f5f9;
                        margin: 0;
                        padding: 24px;
                        direction: rtl;
                        text-align: right;
                        color: #1e293b;
                    }
                    .container {
                        max-width: 540px;
                        margin: 0 auto;
                        background: #ffffff;
                        border-radius: 20px;
                        padding: 36px;
                        box-shadow: 0 10px 25px rgba(0,0,0,0.06);
                        border: 1px solid #e2e8f0;
                    }
                    .header {
                        text-align: center;
                        margin-bottom: 28px;
                    }
                    .brand {
                        font-size: 26px;
                        font-weight: 800;
                        background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%);
                        -webkit-background-clip: text;
                        -webkit-text-fill-color: #6366f1;
                        letter-spacing: -0.5px;
                        margin: 0;
                    }
                    .greeting {
                        font-size: 19px;
                        font-weight: 700;
                        margin-top: 0;
                        margin-bottom: 16px;
                        color: #0f172a;
                    }
                    .text {
                        font-size: 15px;
                        line-height: 1.7;
                        color: #475569;
                        margin-bottom: 24px;
                    }
                    .btn-wrapper {
                        text-align: center;
                        margin: 32px 0;
                    }
                    .btn {
                        display: inline-block;
                        background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%);
                        color: #ffffff !important;
                        text-decoration: none;
                        padding: 14px 36px;
                        border-radius: 12px;
                        font-weight: 700;
                        font-size: 16px;
                        box-shadow: 0 4px 14px rgba(99, 102, 241, 0.35);
                    }
                    .fallback {
                        background: #f8fafc;
                        border: 1px dashed #cbd5e1;
                        border-radius: 10px;
                        padding: 14px;
                        font-size: 13px;
                        color: #64748b;
                        word-break: break-all;
                        line-height: 1.5;
                        margin-bottom: 24px;
                    }
                    .fallback a {
                        color: #6366f1;
                        text-decoration: underline;
                    }
                    .divider {
                        border: none;
                        border-top: 1px solid #e2e8f0;
                        margin: 24px 0;
                    }
                    .footer {
                        font-size: 12px;
                        color: #94a3b8;
                        text-align: center;
                        line-height: 1.5;
                    }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <div class="brand">تمكين | TAMKEEN</div>
                    </div>
                    <div class="greeting">مرحباً {{safeUserName}}،</div>
                    <p class="text">
                        لقد تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بحسابك على منصة تمكين.
                        لتعيين كلمة مرور جديدة، يرجى الضغط على الزر أدناه:
                    </p>
                    <div class="btn-wrapper">
                        <a href="{{safeResetLink}}" class="btn">إعادة تعيين كلمة المرور</a>
                    </div>
                    <div class="fallback">
                        إذا لم يعمل الزر معك، يمكنك نسخ الرابط التالي ولصقه في متصفحك:<br>
                        <a href="{{safeResetLink}}">{{safeResetLink}}</a>
                    </div>
                    <hr class="divider">
                    <div class="footer">
                        هذا الرابط صالح للاستخدام مرة واحدة.<br>
                        إذا لم تكن قد طلبت إعادة تعيين كلمة المرور، يمكنك تجاهل هذا البريد الإلكتروني بأمان وسيظل حسابك محمياً.
                    </div>
                </div>
            </body>
            </html>
            """;

        return SendEmailAsync(email.Value, subject, body, cancellationToken);
    }

    public Task SendVerificationEmailAsync(
        DomainEmail email, string userName, string verificationLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "تأكيد بريدك الإلكتروني - تمكين";
        var safeUserName = WebUtility.HtmlEncode(userName?.Trim() ?? "المستخدم");
        var safeLink = WebUtility.HtmlEncode(verificationLink);

        var body = $$"""
            <!DOCTYPE html>
            <html lang="ar" dir="rtl">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>تأكيد البريد الإلكتروني</title>
                <style>
                    body {
                        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                        background-color: #f1f5f9;
                        margin: 0;
                        padding: 24px;
                        direction: rtl;
                        text-align: right;
                        color: #1e293b;
                    }
                    .container {
                        max-width: 540px;
                        margin: 0 auto;
                        background: #ffffff;
                        border-radius: 20px;
                        padding: 36px;
                        box-shadow: 0 10px 25px rgba(0,0,0,0.06);
                        border: 1px solid #e2e8f0;
                    }
                    .header {
                        text-align: center;
                        margin-bottom: 28px;
                    }
                    .brand {
                        font-size: 26px;
                        font-weight: 800;
                        color: #6366f1;
                        margin: 0;
                    }
                    .greeting {
                        font-size: 19px;
                        font-weight: 700;
                        margin-top: 0;
                        margin-bottom: 16px;
                        color: #0f172a;
                    }
                    .text {
                        font-size: 15px;
                        line-height: 1.7;
                        color: #475569;
                        margin-bottom: 24px;
                    }
                    .btn-wrapper {
                        text-align: center;
                        margin: 32px 0;
                    }
                    .btn {
                        display: inline-block;
                        background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%);
                        color: #ffffff !important;
                        text-decoration: none;
                        padding: 14px 36px;
                        border-radius: 12px;
                        font-weight: 700;
                        font-size: 16px;
                        box-shadow: 0 4px 14px rgba(99, 102, 241, 0.35);
                    }
                    .divider {
                        border: none;
                        border-top: 1px solid #e2e8f0;
                        margin: 24px 0;
                    }
                    .footer {
                        font-size: 12px;
                        color: #94a3b8;
                        text-align: center;
                    }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <div class="brand">تمكين | TAMKEEN</div>
                    </div>
                    <div class="greeting">أهلاً بك {{safeUserName}}!</div>
                    <p class="text">
                        شكراً لانضمامك إلى منصة تمكين. يرجى تأكيد بريدك الإلكتروني لتفعيل جميع ميزات حسابك.
                    </p>
                    <div class="btn-wrapper">
                        <a href="{{safeLink}}" class="btn">تأكيد البريد الإلكتروني</a>
                    </div>
                    <hr class="divider">
                    <div class="footer">
                        إذا لم تقم بإنشاء حساب في تمكين، يمكنك تجاهل هذه الرسالة.
                    </div>
                </div>
            </body>
            </html>
            """;

        return SendEmailAsync(email.Value, subject, body, cancellationToken);
    }

    public async Task SendEmailAsync(
        string recipientEmail, string subject, string body,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogWarning("Email (disabled) → {To}: {Subject}. Check SMTP configuration.", recipientEmail, subject);
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.Username))
        {
            _logger.LogError("Cannot send email to {To}: SMTP Host or Username is not configured.", recipientEmail);
            return;
        }

        try
        {
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_options.Username, _options.Password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 20000
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
            _logger.LogInformation("Email sent successfully → {To}: {Subject}", recipientEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}: {Message}", recipientEmail, ex.Message);
        }
    }
}

internal sealed class SmtpOptions
{
    public bool Enabled { get; set; } = false;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}
