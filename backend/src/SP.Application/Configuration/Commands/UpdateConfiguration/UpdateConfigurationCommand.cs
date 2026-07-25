using SP.Application.Abstractions.Messaging;

namespace SP.Application.Configuration.Commands.UpdateConfiguration;

public sealed record UpdateConfigurationCommand(
    string Key,
    string Value,
    string Category,
    bool IsPublic) : ICommand;