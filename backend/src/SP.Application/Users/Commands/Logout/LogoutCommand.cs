using SP.Application.Abstractions.Messaging;

namespace SP.Application.Users.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : ICommand;
