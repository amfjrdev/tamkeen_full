using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SP.Application.Abstractions.Messaging;

public interface IPresenceService
{
    Task SetOnlineAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SetOfflineAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsOnlineAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, bool>> GetOnlineStatusesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    Task SetTypingAsync(Guid userId, Guid receiverId, CancellationToken cancellationToken = default);
    Task<bool> IsTypingAsync(Guid userId, Guid receiverId, CancellationToken cancellationToken = default);
}

public interface IPushNotificationService
{
    Task SendAsync(
        Guid userId,
        string title,
        string body,
        CancellationToken cancellationToken = default);
}