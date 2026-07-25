using SP.Application.Abstractions.Messaging;

namespace SP.Application.Users.Commands;

public sealed record VerifyUserEmailCommand(Guid UserId) : ICommand;