using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SP.Domain.Users;

namespace SP.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await Context.Users
            .IgnoreQueryFilters()   // allow finding blocked/deleted users during auth
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await Context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        => await Context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                u => u.RefreshTokens.Any(t => t.Token == refreshToken),
                cancellationToken);

    public async Task<IReadOnlyList<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        => await Context.Users
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
        => await Context.Users
            .AsNoTracking()
            .Where(u => u.Role == role)
            .ToListAsync(cancellationToken);

    public async Task HardDeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // 1. Delete standalone chat messages and conversations
        var relatedConversations = await Context.Conversations
            .Where(c => c.Participant1Id == userId || c.Participant2Id == userId)
            .ToListAsync(cancellationToken);
        var relatedConversationIds = relatedConversations.Select(c => c.Id).ToList();

        var relatedChats = await Context.ChatMessagesStandalone
            .Where(m => m.SenderId == userId || relatedConversationIds.Contains(m.ConversationId))
            .ToListAsync(cancellationToken);
        Context.ChatMessagesStandalone.RemoveRange(relatedChats);
        Context.Conversations.RemoveRange(relatedConversations);

        // 2. Delete bookings
        var relatedBookings = await Context.Bookings
            .Where(b => b.ClientId == userId || b.ProviderId == userId)
            .ToListAsync(cancellationToken);
        Context.Bookings.RemoveRange(relatedBookings);

        // 3. Delete payments
        var relatedPayments = await Context.Payments
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
        Context.Payments.RemoveRange(relatedPayments);

        // 4. Delete services
        var relatedServices = await Context.Services
            .Where(s => s.ProviderId == userId)
            .ToListAsync(cancellationToken);
        Context.Services.RemoveRange(relatedServices);

        // 5. Delete provider portfolios, profiles, and transactions
        var relatedPortfolios = await Context.Portfolios
            .Where(p => p.ProviderId == userId)
            .ToListAsync(cancellationToken);
        Context.Portfolios.RemoveRange(relatedPortfolios);

        var relatedProfiles = await Context.ProviderProfiles
            .Where(p => p.ProviderId == userId)
            .ToListAsync(cancellationToken);
        Context.ProviderProfiles.RemoveRange(relatedProfiles);

        var relatedTransactions = await Context.ConnectTransactions
            .Where(t => t.UserId == userId)
            .ToListAsync(cancellationToken);
        Context.ConnectTransactions.RemoveRange(relatedTransactions);

        // 6. Delete wallets, notifications, and user
        var relatedWallets = await Context.Wallets
            .Where(w => w.UserId == userId)
            .ToListAsync(cancellationToken);
        Context.Wallets.RemoveRange(relatedWallets);

        var relatedNotifications = await Context.Notifications
            .Where(n => n.UserId == userId)
            .ToListAsync(cancellationToken);
        Context.Notifications.RemoveRange(relatedNotifications);

        var user = await Context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is not null)
        {
            Context.Users.Remove(user);
        }
    }
}
