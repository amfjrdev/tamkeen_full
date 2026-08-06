using System;
using System.Collections.Generic;
using SP.Domain.Shared;
using SP.Domain.Users;

namespace SP.Application.Users.Dto;

public sealed record RecentBookingDto(
    Guid Id,
    Guid ProviderId,
    string ProviderName,
    string ProviderAvatarUrl,
    Guid ServiceId,
    string ServiceName,
    decimal Price,
    double? Rating,
    DateTime ScheduledDate,
    DateTime CreatedAt
);

public sealed record UserResponseDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    bool IsEmailVerified,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string Role,
    Image UserProfilePicture,
    string? Location,
    int BookingsCount,
    int ReviewsCount,
    int FavoritesCount,
    IReadOnlyList<RecentBookingDto>? RecentBookings,
    decimal HourlyRate = 0
);

public static class UserMapper
{
    public static UserResponseDto ToResponse(this User user)
    {
        return new UserResponseDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.IsEmailVerified,
            user.CreatedAt,
            user.UpdatedAt,
            user.Role.ToString(),
            user.UserProfilePicture,
            null,
            0,
            0,
            0,
            null
        );
    }
}