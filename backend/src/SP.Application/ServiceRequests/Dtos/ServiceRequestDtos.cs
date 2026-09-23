using System;
using System.Collections.Generic;

namespace SP.Application.ServiceRequests.Dtos;

public sealed record ServiceRequestSummaryDto(
    Guid Id,
    Guid ClientId,
    string ClientName,
    string? ClientAvatarUrl,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Description,
    string Wilaya,
    decimal? Budget,
    string Status,
    string? RejectionReason,
    Guid? SelectedProviderId,
    string? SelectedProviderName,
    int ApplicationCount,
    DateTime CreatedAt,
    DateTime? ApprovedAt,
    DateTime? CompletedAt);

public sealed record ServiceRequestApplicationDto(
    Guid Id,
    Guid ServiceRequestId,
    Guid ProviderId,
    string ProviderName,
    string? ProviderAvatarUrl,
    double ProviderRating,
    int ProviderReviewCount,
    string CoverLetter,
    decimal? ProposedPrice,
    int ConnectsSpent,
    string Status,
    DateTime CreatedAt);

public sealed record ServiceRequestReviewDto(
    Guid Id,
    Guid ServiceRequestId,
    Guid ClientId,
    Guid ProviderId,
    int Rating,
    string Comment,
    DateTime CreatedAt);

public sealed record ServiceRequestDetailDto(
    Guid Id,
    Guid ClientId,
    string ClientName,
    string? ClientAvatarUrl,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Description,
    string Wilaya,
    decimal? Budget,
    string Status,
    string? RejectionReason,
    Guid? SelectedProviderId,
    string? SelectedProviderName,
    Guid? SelectedApplicationId,
    int ApplicationCount,
    DateTime CreatedAt,
    DateTime? ApprovedAt,
    DateTime? CompletedAt,
    IReadOnlyList<ServiceRequestApplicationDto> Applications,
    ServiceRequestReviewDto? Review);

public sealed record CreateServiceRequestRequest(
    Guid CategoryId,
    string Title,
    string Description,
    string Wilaya,
    decimal? Budget);

public sealed record ApplyToServiceRequestRequest(
    string CoverLetter,
    decimal? ProposedPrice);

public sealed record SelectProviderRequest(
    Guid ProviderId,
    Guid ApplicationId);

public sealed record ReviewServiceRequestRequest(
    int Rating,
    string Comment);

public sealed record RejectServiceRequestRequest(
    string? Reason);
