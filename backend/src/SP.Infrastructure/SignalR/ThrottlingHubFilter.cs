using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SP.Infrastructure.SignalR;

public sealed class ThrottlingHubFilter : IHubFilter
{
    private static readonly ConcurrentDictionary<string, List<DateTime>> ConnectionRequests = new();
    private const int MaxRequests = 30; // Max 30 invocations
    private const int WindowSeconds = 60; // Per 60 seconds

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var connectionId = invocationContext.Context.ConnectionId;
        var now = DateTime.UtcNow;

        var requestTimes = ConnectionRequests.GetOrAdd(connectionId, _ => new List<DateTime>());
        lock (requestTimes)
        {
            // Evict items outside the sliding window
            requestTimes.RemoveAll(t => t < now.AddSeconds(-WindowSeconds));

            if (requestTimes.Count >= MaxRequests)
            {
                throw new HubException("Throttling: Too many requests. Please slow down.");
            }

            requestTimes.Add(now);
        }

        return await next(invocationContext);
    }
}
