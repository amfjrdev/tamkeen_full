using System;

namespace SP.Application.Chat.Dtos;

public sealed record MessageResponse(
    Guid Id,
    Guid SenderId,
    string Text,
    DateTime SentAt,
    bool IsRead);
