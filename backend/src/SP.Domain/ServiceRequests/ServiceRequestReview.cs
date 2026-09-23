using System;
using SP.Domain.Abstractions;
using SP.Domain.ServiceRequests.Errors;

namespace SP.Domain.ServiceRequests;

public sealed class ServiceRequestReview : Entity
{
    private ServiceRequestReview() { }

    private ServiceRequestReview(
        Guid id,
        Guid serviceRequestId,
        Guid clientId,
        Guid providerId,
        int rating,
        string comment) : base(id)
    {
        ServiceRequestId = serviceRequestId;
        ClientId = clientId;
        ProviderId = providerId;
        Rating = rating;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ServiceRequestId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid ProviderId { get; private set; }
    public int Rating { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public static Result<ServiceRequestReview> Create(
        Guid serviceRequestId,
        Guid clientId,
        Guid providerId,
        int rating,
        string comment)
    {
        if (rating < 1 || rating > 5)
            return Result.Failure<ServiceRequestReview>(ServiceRequestErrors.InvalidRating);

        return Result.Success(new ServiceRequestReview(
            Guid.NewGuid(),
            serviceRequestId,
            clientId,
            providerId,
            rating,
            comment?.Trim() ?? string.Empty));
    }
}
