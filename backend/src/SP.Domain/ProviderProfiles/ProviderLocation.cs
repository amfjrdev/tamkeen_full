namespace SP.Domain.ProviderProfiles;

public sealed class ProviderLocation
{
    public Guid Id { get; private set; }
    public Guid ProviderProfileId { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public DateTime UpdatedAt { get; private set; }

    private ProviderLocation() { }

    public static ProviderLocation Create(
        Guid providerProfileId,
        double latitude,
        double longitude,
        string city,
        string country)
    {
        return new ProviderLocation
        {
            Id = Guid.NewGuid(),
            ProviderProfileId = providerProfileId,
            Latitude = latitude,
            Longitude = longitude,
            City = city,
            Country = country,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(double latitude, double longitude, string city, string country)
    {
        Latitude = latitude;
        Longitude = longitude;
        City = city;
        Country = country;
        UpdatedAt = DateTime.UtcNow;
    }
}
