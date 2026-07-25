using System;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Chat.Errors;
using SP.Domain.Chat.Repositories;

namespace SP.Application.Chat.Commands.MarkMessageAsRead;

public sealed record MarkMessageAsReadCommand(
    Guid UserId,
    Guid MessageId) : ICommand;

public sealed class MarkMessageAsReadCommandHandler
    : ICommandHandler<MarkMessageAsReadCommand>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkMessageAsReadCommandHandler(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        MarkMessageAsReadCommand command,
        CancellationToken cancellationToken = default)
    {
        var message = await _conversationRepository.GetMessageByIdAsync(command.MessageId, cancellationToken);
        if (message is null)
        {
            return Result.Failure(ChatErrors.MessageNotFound);
        }

        var conversation = await _conversationRepository.GetByIdAsync(message.ConversationId, cancellationToken);
        if (conversation is null)
        {
            return Result.Failure(ChatErrors.ConversationNotFound);
        }

        // Validate user is participant
        if (conversation.Participant1Id != command.UserId && conversation.Participant2Id != command.UserId)
        {
            return Result.Failure(ChatErrors.Unauthorized);
        }

        // Only the recipient can mark a message as read
        if (message.SenderId == command.UserId)
        {
            return Result.Failure(ChatErrors.MessageSenderMismatch);
        }

        if (message.IsRead)
        {
            return Result.Success();
        }

        message.MarkAsRead();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
