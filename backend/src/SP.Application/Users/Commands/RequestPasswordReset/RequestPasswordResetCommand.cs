using SP.Application.Abstractions.Messaging;

namespace SP.Application.Users.Commands.RequestPasswordReset;

public sealed record RequestPasswordResetCommand(string Email) : ICommand;