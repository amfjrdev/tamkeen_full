using System;

namespace SP.Application.Chat.Dtos;

public sealed record ConversationResponse(
    Guid Id,
    Guid OtherUserId,
    string OtherUserName,
    string OtherUserAvatarUrl,
    string? LastMessage,
    DateTime? LastMessageAt,
    int UnreadCount,
    bool IsOtherUserOnline,
    bool IsLocked,
    int UnlockCost,
    bool IsNew);

