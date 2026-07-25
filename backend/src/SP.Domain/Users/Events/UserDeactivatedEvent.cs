using SP.Domain.Abstractions;

namespace SP.Domain.Users.Events;

public sealed record UserDeactivatedEvent(Guid UserId) : DomainEvent;
