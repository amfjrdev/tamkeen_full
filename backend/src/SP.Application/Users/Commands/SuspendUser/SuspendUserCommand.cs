using SP.Application.Abstractions.Messaging;

namespace SP.Application.Users.Commands.SuspendUser;

public sealed record SuspendUserCommand(Guid UserId, string? Reason = null) : ICommand;