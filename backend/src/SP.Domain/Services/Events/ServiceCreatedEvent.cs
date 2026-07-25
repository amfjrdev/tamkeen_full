using SP.Domain.Abstractions;

namespace SP.Domain.Services.Events;

public sealed record ServiceCreatedEvent(Guid ServiceId, Guid ProviderId) : DomainEvent;
public sealed record ServiceActivatedEvent(Guid ServiceId, Guid ProviderId) : DomainEvent;
public sealed record ServiceDeactivatedEvent(Guid ServiceId, Guid ProviderId) : DomainEvent;
