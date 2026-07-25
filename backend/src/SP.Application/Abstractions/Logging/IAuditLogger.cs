using System;

namespace SP.Application.Abstractions.Logging;

public interface IAuditLogger
{
    void Log(string action, Guid? userId, object? details = null);
}
