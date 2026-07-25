using System;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Chat.Repositories;

namespace SP.Application.Chat.Queries.GetMessageById;

public sealed record GetMessageByIdQuery(Guid MessageId) : IQuery<MessageDto?>;

public sealed record MessageDto(
    Guid Id,
    Guid SenderId,
    Guid ConversationId,
    string Text,
    DateTime SentAt,
    bool IsRead);

public sealed class GetMessageByIdQueryHandler : IQueryHandler<GetMessageByIdQuery, MessageDto?>
{
    private readonly IConversationRepository _conversationRepository;

    public GetMessageByIdQueryHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<Result<MessageDto?>> HandleAsync(
        GetMessageByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var message = await _conversationRepository.GetMessageByIdAsync(query.MessageId, cancellationToken);
        if (message is null)
        {
            return Result.Success<MessageDto?>(null);
        }

        var dto = new MessageDto(
            message.Id,
            message.SenderId,
            message.ConversationId,
            message.Text,
            message.SentAt,
            message.IsRead);

        return Result.Success<MessageDto?>(dto);
    }
}
