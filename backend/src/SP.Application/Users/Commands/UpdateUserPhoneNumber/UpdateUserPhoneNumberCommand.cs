using SP.Application.Abstractions.Messaging;

namespace SP.Application.Users.Commands.UpdateUserPhoneNumber;

public sealed record UpdateUserPhoneNumberCommand(Guid UserId, string? PhoneNumber) : ICommand;