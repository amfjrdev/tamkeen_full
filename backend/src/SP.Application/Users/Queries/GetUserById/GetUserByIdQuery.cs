using System;
using System.Collections.Generic;
using System.Linq;
using SP.Application.Abstractions.Messaging;
using SP.Application.Abstractions.Messaging.Queries.GetById;
using SP.Application.Users.Dto;
using SP.Domain.Abstractions;
using SP.Domain.Users;
using SP.Domain.Users.Errors;
using SP.Domain.Bookings;
using SP.Domain.Bookings.Repositories;
using SP.Domain.Services.Repositories;
using SP.Domain.ProviderProfiles.Repositories;
using SP.Application.Abstractions.Authentication;

namespace SP.Application.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : GetByIdQuery<UserResponseDto>(Id);

public sealed class GetUserByIdQueryHandler : GetByIdQueryHandler<User, UserResponseDto>,
      IQueryHandler<GetUserByIdQuery, UserResponseDto?>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IBookingRepository _bookingRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IProviderProfileRepository _profileRepository;

    public GetUserByIdQueryHandler(
        IUserRepository repository, 
        IUserContext userContext,
        IBookingRepository bookingRepository,
        IServiceRepository serviceRepository,
        IProviderProfileRepository profileRepository)
        : base(repository)
    {
        _userRepository = repository;
        _userContext = userContext;
        _bookingRepository = bookingRepository;
        _serviceRepository = serviceRepository;
        _profileRepository = profileRepository;
    }

    public async Task<Result<UserResponseDto?>> HandleAsync(
        GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var targetUser = await _userRepository.GetByIdAsync(query.Id, cancellationToken);
        if (targetUser is null)
        {
            return Result.Failure<UserResponseDto?>(UserErrors.NotFound);
        }

        if (query.Id != _userContext.UserId && _userContext.Role != "Admin" && targetUser.Role != UserRole.Provider)
        {
            return Result.Failure<UserResponseDto?>(new Error("Users.Unauthorized", "You are not authorized to view this profile."));
        }

        int bookingsCount = 0;
        int reviewsCount = 0;
        int favoritesCount = 0;

        var completedBookingsResult = await _bookingRepository.GetBookingHistoryAsync(
            query.Id,
            BookingStatus.Completed,
            null, null, 1, 3,
            cancellationToken);

        var recentBookings = new List<RecentBookingDto>();
        if (completedBookingsResult.Bookings is not null)
        {
            foreach (var b in completedBookingsResult.Bookings)
            {
                var service = await _serviceRepository.GetByIdAsync(b.ServiceId, cancellationToken);
                var provider = await _userRepository.GetByIdAsync(b.ProviderId, cancellationToken);

                var serviceName = service?.Name ?? "Unknown Service";
                var price = service?.Price ?? 0;
                var providerName = provider is not null ? provider.FirstName + " " + provider.LastName : "Unknown Provider";
                var providerAvatar = provider?.UserProfilePicture?.Url ?? "https://ui-avatars.com/api/?background=random";
                double? rating = b.Review?.Rating;

                recentBookings.Add(new RecentBookingDto(
                    b.Id,
                    b.ProviderId,
                    providerName,
                    providerAvatar,
                    b.ServiceId,
                    serviceName,
                    price,
                    rating,
                    b.ScheduledDate,
                    b.CreatedAt));
            }
        }

        string? locationName = "Algiers, Algeria";

        if (targetUser.Role == UserRole.Provider)
        {
            var (_, statsReviews, _) = await _bookingRepository.GetProviderStatsAsync(query.Id, cancellationToken);
            var providerBookings = await _bookingRepository.GetByProviderIdAsync(query.Id, cancellationToken);
            bookingsCount = providerBookings.Count();
            reviewsCount = statsReviews;
        }
        else
        {
            var clientBookings = await _bookingRepository.GetByClientIdAsync(query.Id, cancellationToken);
            bookingsCount = clientBookings.Count();
            reviewsCount = completedBookingsResult.Bookings?.Count(b => b.Review != null) ?? 0;
        }

        var response = new UserResponseDto(
            targetUser.Id,
            targetUser.Email,
            targetUser.FirstName,
            targetUser.LastName,
            targetUser.PhoneNumber,
            targetUser.IsEmailVerified,
            targetUser.CreatedAt,
            targetUser.UpdatedAt,
            targetUser.Role.ToString(),
            targetUser.UserProfilePicture,
            locationName,
            bookingsCount,
            reviewsCount,
            favoritesCount,
            recentBookings
        );

        return Result.Success<UserResponseDto?>(response);
    }

    protected override Error NotFoundError() => UserErrors.NotFound;

    protected override UserResponseDto MapToResponse(User entity) =>
        entity.ToResponse();
}