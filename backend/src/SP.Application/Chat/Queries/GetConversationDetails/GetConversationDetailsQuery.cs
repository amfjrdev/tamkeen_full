using System;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Application.Chat.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Chat.Errors;
using SP.Domain.Chat.Repositories;
using SP.Domain.Users;

namespace SP.Application.Chat.Queries.GetConversationDetails;

public sealed record GetConversationDetailsQuery(Guid UserId, Guid ConversationId) : IQuery<ConversationResponse>;

public sealed class GetConversationDetailsQueryHandler : IQueryHandler<GetConversationDetailsQuery, ConversationResponse>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPresenceService _presenceService;

    public GetConversationDetailsQueryHandler(
        IConversationRepository conversationRepository,
        IUserRepository userRepository,
        IPresenceService presenceService)
    {
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _presenceService = presenceService;
    }

    public async Task<Result<ConversationResponse>> HandleAsync(
        GetConversationDetailsQuery query,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(query.ConversationId, cancellationToken);
        if (conversation is null)
        {
            return Result.Failure<ConversationResponse>(ChatErrors.ConversationNotFound);
        }

        if (conversation.Participant1Id != query.UserId && conversation.Participant2Id != query.UserId)
        {
            return Result.Failure<ConversationResponse>(ChatErrors.Unauthorized);
        }

        var otherParticipantId = conversation.Participant1Id == query.UserId
            ? conversation.Participant2Id
            : conversation.Participant1Id;

        var otherUser = await _userRepository.GetByIdAsync(otherParticipantId, cancellationToken);
        if (otherUser is null)
        {
            return Result.Failure<ConversationResponse>(new Error("User.NotFound", "Other participant not found."));
        }

        var otherName = $"{otherUser.FirstName} {otherUser.LastName}";
        var otherAvatar = otherUser.UserProfilePicture?.Url ?? string.Empty;

        var unreadCountsMap = await _conversationRepository.GetUnreadCountsAsync(query.UserId, new[] { conversation.Id }, cancellationToken);
        unreadCountsMap.TryGetValue(conversation.Id, out var unreadCount);

        var isOnline = await _presenceService.IsOnlineAsync(otherParticipantId, cancellationToken);

        var response = new ConversationResponse(
            conversation.Id,
            otherParticipantId,
            otherName,
            otherAvatar,
            conversation.LastMessageText,
            conversation.LastMessageSentAt,
            unreadCount,
            isOnline,
            conversation.IsLocked,
            conversation.UnlockCost,
            conversation.LastMessageText is null);

        return Result.Success(response);
    }
}
