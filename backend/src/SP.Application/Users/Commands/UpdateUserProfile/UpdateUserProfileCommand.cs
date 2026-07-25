using SP.Application.Abstractions.Messaging;

namespace SP.Application.Users.Commands.UpdateUserProfile;

public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber
) : ICommand;