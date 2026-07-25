using System;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Application.Chat.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Chat;
using SP.Domain.Chat.Errors;
using SP.Domain.Chat.Repositories;

namespace SP.Application.Chat.Commands.SendMessage;

public sealed record SendMessageCommand(
    Guid UserId,
    Guid ConversationId,
    string Text) : ICommand<MessageResponse>;

public sealed class SendMessageCommandHandler
    : ICommandHandler<SendMessageCommand, MessageResponse>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SendMessageCommandHandler(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MessageResponse>> HandleAsync(
        SendMessageCommand command,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(command.ConversationId, cancellationToken);
        if (conversation is null)
        {
            return Result.Failure<MessageResponse>(ChatErrors.ConversationNotFound);
        }

        // Validate user is participant
        if (conversation.Participant1Id != command.UserId && conversation.Participant2Id != command.UserId)
        {
            return Result.Failure<MessageResponse>(ChatErrors.Unauthorized);
        }

        // Validate conversation is not locked (except for the client who initiated it)
        if (conversation.IsLocked && command.UserId != conversation.Participant1Id)
        {
            return Result.Failure<MessageResponse>(ChatErrors.ConversationLocked);
        }

        // Create ChatMessage
        var message = ChatMessage.Create(command.ConversationId, command.UserId, command.Text);

        // Add message to repository
        await _conversationRepository.AddMessageAsync(message, cancellationToken);

        // Update conversation last message
        conversation.UpdateLastMessage(message.Text, message.SenderId, message.SentAt);

        // Save
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map response
        var response = new MessageResponse(
            message.Id,
            message.SenderId,
            message.Text,
            message.SentAt,
            message.IsRead);

        return Result.Success(response);
    }
}
