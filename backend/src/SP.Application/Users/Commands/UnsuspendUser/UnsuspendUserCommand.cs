using SP.Application.Abstractions.Messaging;

namespace SP.Application.Users.Commands.UnsuspendUser;

public sealed record UnsuspendUserCommand(Guid UserId) : ICommand;