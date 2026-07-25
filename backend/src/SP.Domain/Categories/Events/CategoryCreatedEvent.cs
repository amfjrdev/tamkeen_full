using SP.Domain.Abstractions;

namespace SP.Domain.Categories.Events;

public sealed record CategoryCreatedEvent(Guid CategoryId, string Name) : DomainEvent;
public sealed record CategoryUpdatedEvent(Guid CategoryId, string Name) : DomainEvent;