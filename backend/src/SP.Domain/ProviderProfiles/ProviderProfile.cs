using SP.Domain.Abstractions;
using SP.Domain.ProviderProfiles.Errors;

namespace SP.Domain.ProviderProfiles;

public sealed class ProviderProfile : AggregateRoot
{
    private ProviderProfile() { }

    private ProviderProfile(Guid id, Guid providerId) : base(id)
    {
        ProviderId = providerId;
        IsAvailable = false;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ProviderId { get; private set; }
    public string? AboutMe { get; private set; }
    public decimal HourlyRate { get; private set; }
    public bool IsAvailable { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public int ResponseTimeMins { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static ProviderProfile Create(Guid providerId)
    {
        if (providerId == Guid.Empty)
            throw new ArgumentException("ProviderId cannot be empty.", nameof(providerId));

        return new ProviderProfile(Guid.NewGuid(), providerId);
    }

    public Result UpdateDetails(string? aboutMe, decimal hourlyRate, int responseTimeMins)
    {
        if (hourlyRate < 0)
            return Result.Failure(ProviderProfileErrors.InvalidHourlyRate);

        if (responseTimeMins < 0)
            return Result.Failure(ProviderProfileErrors.InvalidResponseTime);

        if (aboutMe is not null && aboutMe.Length > 1000)
            return Result.Failure(ProviderProfileErrors.InvalidAboutMe);

        AboutMe = aboutMe?.Trim();
        HourlyRate = hourlyRate;
        ResponseTimeMins = responseTimeMins;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result SetLocation(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            return Result.Failure(ProviderProfileErrors.InvalidLatitude);

        if (longitude < -180 || longitude > 180)
            return Result.Failure(ProviderProfileErrors.InvalidLongitude);

        Latitude = latitude;
        Longitude = longitude;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
