using Microsoft.Extensions.Logging;
using SP.Application.Abstractions.Messaging;
using StackExchange.Redis;

namespace SP.Infrastructure.Services;

internal sealed class RedisPresenceService : IPresenceService
{
    private readonly IConnectionMultiplexer _redis;

    // How long before "online" key expires automatically.
    // The Hub refreshes it on every connect — so if the key
    // disappears the user is genuinely offline.
    private static readonly TimeSpan OnlineTtl = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan TypingTtl = TimeSpan.FromSeconds(4);

    public RedisPresenceService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task SetOnlineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync($"presence:{userId}", "1", OnlineTtl);
    }

    public async Task SetOfflineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync($"presence:{userId}");
    }

    public async Task<bool> IsOnlineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync($"presence:{userId}");
    }

    public async Task<Dictionary<Guid, bool>> GetOnlineStatusesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var batch = db.CreateBatch();
        var tasks = userIds.Select(id => new { Id = id, Task = batch.KeyExistsAsync($"presence:{id}") }).ToList();
        batch.Execute();

        var results = new Dictionary<Guid, bool>();
        foreach (var item in tasks)
        {
            results[item.Id] = await item.Task;
        }
        return results;
    }

    public async Task SetTypingAsync(Guid userId, Guid receiverId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync($"typing:{userId}:{receiverId}", "1", TypingTtl);
    }

    public async Task<bool> IsTypingAsync(Guid userId, Guid receiverId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync($"typing:{userId}:{receiverId}");
    }
}


internal sealed class StubPushNotificationService : IPushNotificationService
{
    private readonly ILogger<StubPushNotificationService> _logger;

    public StubPushNotificationService(ILogger<StubPushNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(
        Guid userId,
        string title,
        string body,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Push notification (stub) → User {UserId}: [{Title}] {Body}",
            userId, title, body);

        return Task.CompletedTask;
    }
}