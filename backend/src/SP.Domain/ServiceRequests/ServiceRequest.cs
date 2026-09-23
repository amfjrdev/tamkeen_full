using System;
using System.Collections.Generic;
using System.Linq;
using SP.Domain.Abstractions;
using SP.Domain.ServiceRequests.Enums;
using SP.Domain.ServiceRequests.Errors;
using SP.Domain.ServiceRequests.Events;

namespace SP.Domain.ServiceRequests;

public sealed class ServiceRequest : AggregateRoot
{
    private readonly List<ServiceRequestApplication> _applications = [];
    private ServiceRequestReview? _review;

    private ServiceRequest() { }

    private ServiceRequest(
        Guid id,
        Guid clientId,
        Guid categoryId,
        string title,
        string description,
        string wilaya,
        decimal? budget) : base(id)
    {
        ClientId = clientId;
        CategoryId = categoryId;
        Title = title;
        Description = description;
        Wilaya = wilaya;
        Budget = budget;
        Status = ServiceRequestStatus.PendingAdminReview;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ClientId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Wilaya { get; private set; } = string.Empty;
    public decimal? Budget { get; private set; }
    public ServiceRequestStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    public Guid? SelectedProviderId { get; private set; }
    public Guid? SelectedApplicationId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public IReadOnlyList<ServiceRequestApplication> Applications => _applications.AsReadOnly();
    public ServiceRequestReview? Review => _review;

    // ── Factory ────────────────────────────────────────────────────────────

    public static Result<ServiceRequest> Create(
        Guid clientId,
        Guid categoryId,
        string title,
        string description,
        string wilaya,
        decimal? budget = null)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("Client ID cannot be empty.", nameof(clientId));

        if (categoryId == Guid.Empty)
            return Result.Failure<ServiceRequest>(ServiceRequestErrors.InvalidCategory);

        if (string.IsNullOrWhiteSpace(title) || title.Length > 120)
            return Result.Failure<ServiceRequest>(ServiceRequestErrors.InvalidTitle);

        if (string.IsNullOrWhiteSpace(description) || description.Length > 2000)
            return Result.Failure<ServiceRequest>(ServiceRequestErrors.InvalidDescription);

        if (string.IsNullOrWhiteSpace(wilaya) || wilaya.Length > 50)
            return Result.Failure<ServiceRequest>(ServiceRequestErrors.InvalidWilaya);

        if (budget.HasValue && budget.Value < 0)
            return Result.Failure<ServiceRequest>(ServiceRequestErrors.InvalidBudget);

        var request = new ServiceRequest(
            Guid.NewGuid(),
            clientId,
            categoryId,
            title.Trim(),
            description.Trim(),
            wilaya.Trim(),
            budget);

        request.RaiseDomainEvent(new ServiceRequestCreatedEvent(request.Id, request.ClientId, request.CategoryId));
        return Result.Success(request);
    }

    // ── Moderation ─────────────────────────────────────────────────────────

    public Result Approve()
    {
        if (Status != ServiceRequestStatus.PendingAdminReview)
            return Result.Failure(ServiceRequestErrors.NotPendingReview);

        Status = ServiceRequestStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RejectionReason = null;

        RaiseDomainEvent(new ServiceRequestApprovedEvent(Id, ClientId));
        return Result.Success();
    }

    public Result Reject(string? reason)
    {
        if (Status != ServiceRequestStatus.PendingAdminReview)
            return Result.Failure(ServiceRequestErrors.NotPendingReview);

        Status = ServiceRequestStatus.Rejected;
        RejectionReason = reason?.Trim();
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ServiceRequestRejectedEvent(Id, ClientId, RejectionReason));
        return Result.Success();
    }

    // ── Provider Application ───────────────────────────────────────────────

    public Result<ServiceRequestApplication> AddApplication(
        Guid providerId,
        string providerWilaya,
        string coverLetter,
        decimal? proposedPrice,
        int connectsSpent = 10)
    {
        if (Status != ServiceRequestStatus.Approved)
            return Result.Failure<ServiceRequestApplication>(ServiceRequestErrors.NotApproved);

        if (!string.Equals(Wilaya, providerWilaya, StringComparison.OrdinalIgnoreCase))
            return Result.Failure<ServiceRequestApplication>(ServiceRequestErrors.WilayaMismatch);

        if (SelectedProviderId.HasValue)
            return Result.Failure<ServiceRequestApplication>(ServiceRequestErrors.ProviderAlreadySelected);

        if (_applications.Any(a => a.ProviderId == providerId))
            return Result.Failure<ServiceRequestApplication>(ServiceRequestErrors.AlreadyApplied);

        var appResult = ServiceRequestApplication.Create(Id, providerId, coverLetter, proposedPrice, connectsSpent);
        if (appResult.IsFailure)
            return appResult;

        _applications.Add(appResult.Value);
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProviderAppliedEvent(Id, appResult.Value.Id, providerId, connectsSpent));
        return Result.Success(appResult.Value);
    }

    // ── Provider Selection ─────────────────────────────────────────────────

    public Result SelectProvider(Guid providerId, Guid applicationId)
    {
        if (Status != ServiceRequestStatus.Approved)
            return Result.Failure(ServiceRequestErrors.NotApproved);

        if (SelectedProviderId.HasValue)
            return Result.Failure(ServiceRequestErrors.ProviderAlreadySelected);

        var application = _applications.FirstOrDefault(a => a.Id == applicationId && a.ProviderId == providerId);
        if (application is null)
            return Result.Failure(ServiceRequestErrors.ApplicationNotFound);

        SelectedProviderId = providerId;
        SelectedApplicationId = applicationId;
        Status = ServiceRequestStatus.ProviderSelected;
        UpdatedAt = DateTime.UtcNow;

        foreach (var app in _applications)
        {
            if (app.Id == applicationId)
                app.Accept();
            else
                app.Reject();
        }

        RaiseDomainEvent(new ProviderSelectedEvent(Id, ClientId, providerId, applicationId));
        return Result.Success();
    }

    // ── Completion & Cancellation ──────────────────────────────────────────

    public Result Complete()
    {
        if (Status != ServiceRequestStatus.ProviderSelected)
            return Result.Failure(ServiceRequestErrors.NotInProviderSelectedState);

        Status = ServiceRequestStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ServiceRequestCompletedEvent(Id, ClientId, SelectedProviderId!.Value));
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == ServiceRequestStatus.Completed)
            return Result.Failure(new Error("ServiceRequest.AlreadyCompleted", "Cannot cancel a completed service request."));

        Status = ServiceRequestStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    // ── Review ─────────────────────────────────────────────────────────────

    public Result AddReview(Guid clientId, int rating, string comment)
    {
        if (Status != ServiceRequestStatus.Completed)
            return Result.Failure(ServiceRequestErrors.NotCompleted);

        if (ClientId != clientId)
            return Result.Failure(ServiceRequestErrors.UnauthorizedAction);

        if (_review is not null)
            return Result.Failure(ServiceRequestErrors.AlreadyReviewed);

        if (!SelectedProviderId.HasValue)
            return Result.Failure(ServiceRequestErrors.NotInProviderSelectedState);

        var reviewResult = ServiceRequestReview.Create(Id, ClientId, SelectedProviderId.Value, rating, comment);
        if (reviewResult.IsFailure)
            return Result.Failure(reviewResult.Error);

        _review = reviewResult.Value;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ServiceRequestReviewedEvent(Id, SelectedProviderId.Value, rating));
        return Result.Success();
    }
}
