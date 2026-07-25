// SP.Infrastructure/Persistence/Repositories/NotificationRepository.cs

using Microsoft.EntityFrameworkCore;
using SP.Domain.Notifications;
using SP.Domain.Notifications.Repositories;

namespace SP.Infrastructure.Persistence.Repositories;

internal sealed class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
        => await Context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
}
