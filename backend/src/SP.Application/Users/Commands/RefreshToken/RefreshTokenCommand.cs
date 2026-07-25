using SP.Application.Abstractions.Authentication;
using SP.Application.Abstractions.Messaging;

namespace SP.Application.Users.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AuthenticationResult>;