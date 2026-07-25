using System;
using SP.Application.Abstractions.Messaging;

namespace SP.Application.Users.Commands.UpdateUserLocation;

public sealed record UpdateUserLocationCommand(
    Guid UserId,
    double Latitude,
    double Longitude
) : ICommand;
