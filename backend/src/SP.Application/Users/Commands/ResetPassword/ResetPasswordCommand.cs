using SP.Application.Abstractions.Messaging;

namespace SP.Application.Users.Commands.ResetPassword;

public sealed record ResetPasswordCommand(string Token, string NewPassword) : ICommand;