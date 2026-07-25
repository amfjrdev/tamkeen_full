using System;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Application.Chat.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Chat;
using SP.Domain.Chat.Repositories;
using SP.Domain.Users;

namespace SP.Application.Chat.Commands.CreateConversation;

public sealed record CreateConversationCommand(
    Guid UserId,
    Guid OtherUserId) : ICommand<ConversationResponse>;

public sealed class CreateConversationCommandHandler
    : ICommandHandler<CreateConversationCommand, ConversationResponse>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateConversationCommandHandler(
        IConversationRepository conversationRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ConversationResponse>> HandleAsync(
        CreateConversationCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.UserId == command.OtherUserId)
            return Result.Failure<ConversationResponse>(new Error("Conversation.SameUser", "Cannot create conversation with yourself"));

        var otherUser = await _userRepository.GetByIdAsync(command.OtherUserId, cancellationToken);
        if (otherUser is null)
            return Result.Failure<ConversationResponse>(new Error("User.NotFound", "User not found"));

        // Check if conversation already exists
        var existing = await _conversationRepository.GetByParticipantsAsync(command.UserId, command.OtherUserId, cancellationToken);
        if (existing is not null)
        {
            return Result.Success(MapToResponse(existing, otherUser));
        }

        // Create new conversation
        var conversation = Conversation.Create(command.UserId, command.OtherUserId);
        await _conversationRepository.AddAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToResponse(conversation, otherUser));
    }

    private static ConversationResponse MapToResponse(Conversation conversation, User otherUser)
    {
        var otherUserId = conversation.Participant1Id == otherUser.Id
            ? conversation.Participant1Id
            : conversation.Participant2Id;

        return new ConversationResponse(
            conversation.Id,
            otherUserId,
            $"{otherUser.FirstName} {otherUser.LastName}",
            otherUser.UserProfilePicture?.Url ?? string.Empty,
            conversation.LastMessageText,
            conversation.LastMessageSentAt,
            0,
            false,
            conversation.IsLocked,
            conversation.UnlockCost,
            conversation.LastMessageText is null);
    }
}