using SP.Domain.Abstractions;

namespace SP.Domain.ProviderProfiles.Errors;

public static class ProviderProfileErrors
{
    public static readonly Error NotFound =
        new("ProviderProfile.NotFound", "Provider profile not found.");

    public static readonly Error AlreadyExists =
        new("ProviderProfile.AlreadyExists", "A profile already exists for this provider.");

    public static readonly Error InvalidHourlyRate =
        new("ProviderProfile.InvalidHourlyRate", "Hourly rate cannot be negative.");

    public static readonly Error InvalidResponseTime =
        new("ProviderProfile.InvalidResponseTime", "Response time cannot be negative.");

    public static readonly Error InvalidAboutMe =
        new("ProviderProfile.InvalidAboutMe", "About me cannot exceed 1000 characters.");

    public static readonly Error InvalidLatitude =
        new("ProviderProfile.InvalidLatitude", "Latitude must be between -90 and 90.");

    public static readonly Error InvalidLongitude =
        new("ProviderProfile.InvalidLongitude", "Longitude must be between -180 and 180.");
}
