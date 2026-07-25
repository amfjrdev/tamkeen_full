using System;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Chat.Repositories;

namespace SP.Application.Chat.Queries.GetConversationById;

public sealed record GetConversationByIdQuery(Guid ConversationId) : IQuery<ConversationDto?>;

public sealed record ConversationDto(
    Guid Id,
    Guid Participant1Id,
    Guid Participant2Id,
    bool IsLocked,
    int UnlockCost);

public sealed class GetConversationByIdQueryHandler : IQueryHandler<GetConversationByIdQuery, ConversationDto?>
{
    private readonly IConversationRepository _conversationRepository;

    public GetConversationByIdQueryHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<Result<ConversationDto?>> HandleAsync(
        GetConversationByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(query.ConversationId, cancellationToken);
        if (conversation is null)
        {
            return Result.Success<ConversationDto?>(null);
        }

        var dto = new ConversationDto(
            conversation.Id,
            conversation.Participant1Id,
            conversation.Participant2Id,
            conversation.IsLocked,
            conversation.UnlockCost);

        return Result.Success<ConversationDto?>(dto);
    }
}
