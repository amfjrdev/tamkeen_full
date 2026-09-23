using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Application.ServiceRequests.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Bookings.Repositories;
using SP.Domain.Categories.Repositories;
using SP.Domain.ProviderProfiles.Repositories;
using SP.Domain.ServiceRequests.Errors;
using SP.Domain.ServiceRequests.Repositories;
using SP.Domain.Users;
using SP.Domain.Users;

namespace SP.Application.ServiceRequests.Queries.GetServiceRequestDetails;

public sealed record GetServiceRequestDetailsQuery(
    Guid RequestId,
    Guid CurrentUserId) : IQuery<ServiceRequestDetailDto>;

public sealed class GetServiceRequestDetailsQueryHandler
    : IQueryHandler<GetServiceRequestDetailsQuery, ServiceRequestDetailDto>
{
    private readonly IServiceRequestRepository _requestRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBookingRepository _bookingRepository;

    public GetServiceRequestDetailsQueryHandler(
        IServiceRequestRepository requestRepository,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        IBookingRepository bookingRepository)
    {
        _requestRepository = requestRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<ServiceRequestDetailDto>> HandleAsync(
        GetServiceRequestDetailsQuery query,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdWithApplicationsAsync(query.RequestId, cancellationToken);
        if (request is null)
            return Result.Failure<ServiceRequestDetailDto>(ServiceRequestErrors.NotFound);

        var client = await _userRepository.GetByIdAsync(request.ClientId, cancellationToken);
        var clientName = client != null ? $"{client.FirstName} {client.LastName}".Trim() : string.Empty;
        var clientAvatar = client?.UserProfilePicture?.Url;

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        var categoryName = category?.Name ?? "Service";

        string? selectedProviderName = null;
        if (request.SelectedProviderId.HasValue)
        {
            var pUser = await _userRepository.GetByIdAsync(request.SelectedProviderId.Value, cancellationToken);
            if (pUser != null)
                selectedProviderName = $"{pUser.FirstName} {pUser.LastName}".Trim();
        }

        var isClient = request.ClientId == query.CurrentUserId;
        var currentUser = await _userRepository.GetByIdAsync(query.CurrentUserId, cancellationToken);
        var isAdmin = currentUser?.Role == UserRole.Admin;

        var applicationDtos = new List<ServiceRequestApplicationDto>();

        // Only client, admin, or the applying provider can view applications
        foreach (var app in request.Applications)
        {
            if (!isClient && !isAdmin && app.ProviderId != query.CurrentUserId)
                continue;

            var providerUser = await _userRepository.GetByIdAsync(app.ProviderId, cancellationToken);
            var pName = providerUser != null ? $"{providerUser.FirstName} {providerUser.LastName}".Trim() : "Provider";
            var pAvatar = providerUser?.UserProfilePicture?.Url;

            var (_, reviewCount, avgRating) = await _bookingRepository.GetProviderStatsAsync(app.ProviderId, cancellationToken);

            applicationDtos.Add(new ServiceRequestApplicationDto(
                app.Id,
                app.ServiceRequestId,
                app.ProviderId,
                pName,
                pAvatar,
                avgRating,
                reviewCount,
                app.CoverLetter,
                app.ProposedPrice,
                app.ConnectsSpent,
                app.Status.ToString(),
                app.CreatedAt));
        }

        ServiceRequestReviewDto? reviewDto = null;
        if (request.Review is not null)
        {
            reviewDto = new ServiceRequestReviewDto(
                request.Review.Id,
                request.Review.ServiceRequestId,
                request.Review.ClientId,
                request.Review.ProviderId,
                request.Review.Rating,
                request.Review.Comment,
                request.Review.CreatedAt);
        }

        var detailDto = new ServiceRequestDetailDto(
            request.Id,
            request.ClientId,
            clientName,
            clientAvatar,
            request.CategoryId,
            categoryName,
            request.Title,
            request.Description,
            request.Wilaya,
            request.Budget,
            request.Status.ToString(),
            request.RejectionReason,
            request.SelectedProviderId,
            selectedProviderName,
            request.SelectedApplicationId,
            request.Applications.Count,
            request.CreatedAt,
            request.ApprovedAt,
            request.CompletedAt,
            applicationDtos,
            reviewDto);

        return Result.Success(detailDto);
    }
}
