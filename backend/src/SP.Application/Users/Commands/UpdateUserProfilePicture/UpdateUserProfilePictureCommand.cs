using SP.Application.Abstractions.Messaging;

namespace SP.Application.Users.Commands.UpdateUserProfilePicture;

public sealed record UpdateUserProfilePictureCommand(
    Guid UserId,
    string PictureUrl
) : ICommand;
