using SP.Application.Abstractions.Messaging;

namespace SP.Application.Users.Commands.BlockUser;

public sealed record BlockUserCommand(Guid UserId, Guid BlockedUserId, string? Reason = null) : ICommand;