using SP.Application.Abstractions.Messaging;

namespace SP.Application.Users.Commands.DeleteUserAccount;

public sealed record DeleteUserAccountCommand(
    Guid UserId,
    string ConfirmationText,
    string Password
) : ICommand;