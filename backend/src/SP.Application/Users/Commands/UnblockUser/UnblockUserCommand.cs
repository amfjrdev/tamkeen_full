using SP.Application.Abstractions.Messaging;

namespace SP.Application.Users.Commands.UnblockUser;

public sealed record UnblockUserCommand(Guid UserId) : ICommand;