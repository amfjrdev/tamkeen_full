using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SP.Application.Abstractions.Logging;

namespace SP.Infrastructure.Logging;

public sealed class AuditLogger : IAuditLogger
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLogger> _logger;

    public AuditLogger(IHttpContextAccessor httpContextAccessor, ILogger<AuditLogger> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public void Log(string action, Guid? userId, object? details = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var correlationId = httpContext?.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString("N");
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var auditEntry = new
        {
            Timestamp = DateTime.UtcNow.ToString("o"),
            Action = action,
            UserId = userId?.ToString() ?? "unauthenticated",
            IpAddress = ipAddress,
            CorrelationId = correlationId,
            Details = details
        };

        // Write as structured JSON log stream entry
        _logger.LogInformation("AUDIT: {AuditJson}", JsonSerializer.Serialize(auditEntry));
    }
}
