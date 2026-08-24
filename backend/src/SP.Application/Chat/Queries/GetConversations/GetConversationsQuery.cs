using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Application.Chat.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Chat.Repositories;
using SP.Domain.Users;

namespace SP.Application.Chat.Queries.GetConversations;

public sealed record GetConversationsQuery(Guid UserId) : IQuery<IReadOnlyList<ConversationResponse>>;

public sealed class GetConversationsQueryHandler
    : IQueryHandler<GetConversationsQuery, IReadOnlyList<ConversationResponse>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPresenceService _presenceService;

    public GetConversationsQueryHandler(
        IConversationRepository conversationRepository,
        IUserRepository userRepository,
        IPresenceService presenceService)
    {
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _presenceService = presenceService;
    }

    public async Task<Result<IReadOnlyList<ConversationResponse>>> HandleAsync(
        GetConversationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var conversations = await _conversationRepository.GetUserConversationsAsync(query.UserId, cancellationToken);
        if (conversations.Count == 0)
        {
            return Result.Success<IReadOnlyList<ConversationResponse>>(new List<ConversationResponse>());
        }

        var otherUserIds = conversations
            .Select(c => c.Participant1Id == query.UserId ? c.Participant2Id : c.Participant1Id)
            .Distinct()
            .ToList();

        var conversationIds = conversations.Select(c => c.Id).ToList();

        var otherUsers = await _userRepository.GetByIdsAsync(otherUserIds, cancellationToken);
        var usersMap = otherUsers.ToDictionary(u => u.Id);

        var unreadCountsMap = await _conversationRepository.GetUnreadCountsAsync(query.UserId, conversationIds, cancellationToken);
        var onlineStatusesMap = await _presenceService.GetOnlineStatusesAsync(otherUserIds, cancellationToken);

        var responses = new List<ConversationResponse>();

        foreach (var conv in conversations)
        {
            if (conv.LastMessageText is null)
                continue;

            var otherParticipantId = conv.Participant1Id == query.UserId ? conv.Participant2Id : conv.Participant1Id;

            if (!usersMap.TryGetValue(otherParticipantId, out var otherUser))
                continue;

            var otherName = $"{otherUser.FirstName} {otherUser.LastName}";
            var otherAvatar = otherUser.UserProfilePicture.Url;

            unreadCountsMap.TryGetValue(conv.Id, out var unreadCount);
            onlineStatusesMap.TryGetValue(otherParticipantId, out var isOnline);

            responses.Add(new ConversationResponse(
                conv.Id,
                otherParticipantId,
                otherName,
                otherAvatar,
                conv.LastMessageText,
                conv.LastMessageSentAt,
                unreadCount,
                isOnline,
                conv.IsLocked,
                conv.UnlockCost,
                conv.LastMessageText is null));
        }

        return Result.Success<IReadOnlyList<ConversationResponse>>(responses);
    }
}
