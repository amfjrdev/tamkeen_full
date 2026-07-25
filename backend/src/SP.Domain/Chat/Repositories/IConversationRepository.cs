using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SP.Domain.Abstractions;

namespace SP.Domain.Chat.Repositories;

public interface IConversationRepository : IRepository<Conversation>
{
    new Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Conversation?> GetByParticipantsAsync(Guid p1, Guid p2, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Conversation>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> GetUnreadCountsAsync(Guid userId, IEnumerable<Guid> conversationIds, CancellationToken cancellationToken = default);
    Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid conversationId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetMessagesCountAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task<ChatMessage?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken = default);
}
