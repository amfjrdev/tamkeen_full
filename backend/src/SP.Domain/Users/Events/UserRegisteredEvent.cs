using SP.Domain.Abstractions;

namespace SP.Domain.Users.Events;

public sealed record UserRegisteredEvent(
    Guid UserId,
    string Email,
    UserRole Role) : DomainEvent;

public sealed record UserEmailVerifiedEvent(
    Guid UserId,
    string Email) : DomainEvent;


public sealed record UserBlockedEvent(
    Guid UserId,
    string? Reason) : DomainEvent;

public sealed record UserUnblockedEvent(
    Guid UserId) : DomainEvent;

public sealed record UserSuspendedEvent(
    Guid UserId,
    string? Reason) : DomainEvent;

public sealed record UserUnsuspendedEvent(
    Guid UserId) : DomainEvent;

public sealed record UserDeletedEvent(
    Guid UserId) : DomainEvent;

public sealed record UserProfilePictureUpdatedDomainEvent(
    Guid UserId, string newProfilePictureUrl): DomainEvent;