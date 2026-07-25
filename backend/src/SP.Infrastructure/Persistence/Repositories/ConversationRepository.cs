using Microsoft.EntityFrameworkCore;
using SP.Domain.Chat;
using SP.Domain.Chat.Repositories;

namespace SP.Infrastructure.Persistence.Repositories;

internal sealed class ConversationRepository : Repository<Conversation>, IConversationRepository
{
    public ConversationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Conversation?> GetByParticipantsAsync(Guid p1, Guid p2, CancellationToken cancellationToken = default)
        => await Context.Conversations
            .FirstOrDefaultAsync(c =>
                (c.Participant1Id == p1 && c.Participant2Id == p2) ||
                (c.Participant1Id == p2 && c.Participant2Id == p1),
                cancellationToken);

    public async Task<IReadOnlyList<Conversation>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default)
        => await Context.Conversations
            .Where(c => c.Participant1Id == userId || c.Participant2Id == userId)
            .OrderByDescending(c => c.LastMessageSentAt ?? c.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
        => await Context.ChatMessagesStandalone
            .CountAsync(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead, cancellationToken);

    public async Task<Dictionary<Guid, int>> GetUnreadCountsAsync(Guid userId, IEnumerable<Guid> conversationIds, CancellationToken cancellationToken = default)
    {
        var list = await Context.ChatMessagesStandalone
            .Where(m => conversationIds.Contains(m.ConversationId) && m.SenderId != userId && !m.IsRead)
            .GroupBy(m => m.ConversationId)
            .Select(g => new { ConversationId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return list.ToDictionary(x => x.ConversationId, x => x.Count);
    }

    public async Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default)
        => await Context.ChatMessagesStandalone.AddAsync(message, cancellationToken);

    // Returns messages in ascending order (oldest first) for correct chat display
    public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid conversationId, int page, int pageSize, CancellationToken cancellationToken = default)
        => await Context.ChatMessagesStandalone
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task<int> GetMessagesCountAsync(Guid conversationId, CancellationToken cancellationToken = default)
        => await Context.ChatMessagesStandalone
            .CountAsync(m => m.ConversationId == conversationId, cancellationToken);

    public async Task<ChatMessage?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken = default)
        => await Context.ChatMessagesStandalone
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);
}
