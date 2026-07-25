using SP.Application.Abstractions.Authentication;
using SP.Application.Abstractions.Messaging;

namespace SP.Application.Users.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<AuthenticationResult>;