using SP.Application.Abstractions.Messaging;
using SP.Application.Chat.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Chat.Errors;
using SP.Domain.Chat.Repositories;

namespace SP.Application.Chat.Queries.GetMessages;

public sealed record GetMessagesResponse(
    IReadOnlyList<MessageResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);

public sealed record GetMessagesQuery(
    Guid UserId,
    Guid ConversationId,
    int Page,
    int PageSize) : IQuery<GetMessagesResponse>;

public sealed class GetMessagesQueryHandler
    : IQueryHandler<GetMessagesQuery, GetMessagesResponse>
{
    private readonly IConversationRepository _conversationRepository;

    public GetMessagesQueryHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<Result<GetMessagesResponse>> HandleAsync(
        GetMessagesQuery query,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(query.ConversationId, cancellationToken);
        if (conversation is null)
            return Result.Failure<GetMessagesResponse>(ChatErrors.ConversationNotFound);

        if (conversation.Participant1Id != query.UserId && conversation.Participant2Id != query.UserId)
            return Result.Failure<GetMessagesResponse>(ChatErrors.Unauthorized);

        var totalCount = await _conversationRepository.GetMessagesCountAsync(query.ConversationId, cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        // Returns ascending order (oldest first) — correct for chat display
        var messages = await _conversationRepository.GetMessagesAsync(
            query.ConversationId,
            query.Page,
            query.PageSize,
            cancellationToken);

        var items = messages.Select(m => new MessageResponse(
            m.Id,
            m.SenderId,
            m.Text,
            m.SentAt,
            m.IsRead
        )).ToList();

        return Result.Success(new GetMessagesResponse(
            items,
            query.Page,
            query.PageSize,
            totalCount,
            totalPages,
            HasNextPage: query.Page < totalPages,
            HasPreviousPage: query.Page > 1));
    }
}
