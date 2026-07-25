namespace SP.Domain.Users;

public sealed class UserLocation
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public DateTime UpdatedAt { get; private set; }

    private UserLocation() { }

    public static UserLocation Create(
        Guid userId,
        double latitude,
        double longitude,
        string city,
        string country)
    {
        return new UserLocation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
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
