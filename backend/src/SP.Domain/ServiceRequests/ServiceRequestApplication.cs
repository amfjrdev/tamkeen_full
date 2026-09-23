using System;
using SP.Domain.Abstractions;
using SP.Domain.ServiceRequests.Enums;
using SP.Domain.ServiceRequests.Errors;

namespace SP.Domain.ServiceRequests;

public sealed class ServiceRequestApplication : Entity
{
    private ServiceRequestApplication() { }

    private ServiceRequestApplication(
        Guid id,
        Guid serviceRequestId,
        Guid providerId,
        string coverLetter,
        decimal? proposedPrice,
        int connectsSpent) : base(id)
    {
        ServiceRequestId = serviceRequestId;
        ProviderId = providerId;
        CoverLetter = coverLetter;
        ProposedPrice = proposedPrice;
        ConnectsSpent = connectsSpent;
        Status = RequestApplicationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ServiceRequestId { get; private set; }
    public Guid ProviderId { get; private set; }
    public string CoverLetter { get; private set; } = string.Empty;
    public decimal? ProposedPrice { get; private set; }
    public int ConnectsSpent { get; private set; }
    public RequestApplicationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DecidedAt { get; private set; }

    public static Result<ServiceRequestApplication> Create(
        Guid serviceRequestId,
        Guid providerId,
        string coverLetter,
        decimal? proposedPrice,
        int connectsSpent = 10)
    {
        if (serviceRequestId == Guid.Empty)
            throw new ArgumentException("Service request ID cannot be empty.", nameof(serviceRequestId));

        if (providerId == Guid.Empty)
            throw new ArgumentException("Provider ID cannot be empty.", nameof(providerId));

        if (string.IsNullOrWhiteSpace(coverLetter))
            return Result.Failure<ServiceRequestApplication>(ServiceRequestErrors.InvalidDescription);

        if (proposedPrice.HasValue && proposedPrice.Value < 0)
            return Result.Failure<ServiceRequestApplication>(ServiceRequestErrors.InvalidBudget);

        return Result.Success(new ServiceRequestApplication(
            Guid.NewGuid(),
            serviceRequestId,
            providerId,
            coverLetter.Trim(),
            proposedPrice,
            connectsSpent));
    }

    internal void Accept()
    {
        Status = RequestApplicationStatus.Accepted;
        DecidedAt = DateTime.UtcNow;
    }

    internal void Reject()
    {
        Status = RequestApplicationStatus.Rejected;
        DecidedAt = DateTime.UtcNow;
    }
}
