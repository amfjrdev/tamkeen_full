using System;
using SP.Domain.Abstractions;

namespace SP.Domain.ServiceRequests.Events;

public sealed record ServiceRequestCreatedEvent(Guid RequestId, Guid ClientId, Guid CategoryId) : DomainEvent;
public sealed record ServiceRequestApprovedEvent(Guid RequestId, Guid ClientId) : DomainEvent;
public sealed record ServiceRequestRejectedEvent(Guid RequestId, Guid ClientId, string? Reason) : DomainEvent;
public sealed record ProviderAppliedEvent(Guid RequestId, Guid ApplicationId, Guid ProviderId, int ConnectsSpent) : DomainEvent;
public sealed record ProviderSelectedEvent(Guid RequestId, Guid ClientId, Guid ProviderId, Guid ApplicationId) : DomainEvent;
public sealed record ServiceRequestCompletedEvent(Guid RequestId, Guid ClientId, Guid ProviderId) : DomainEvent;
public sealed record ServiceRequestReviewedEvent(Guid RequestId, Guid ProviderId, int Rating) : DomainEvent;
