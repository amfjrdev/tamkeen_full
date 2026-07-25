using SP.Domain.Abstractions;

namespace SP.Domain.Shared;

public sealed class AppConfiguration : Entity
{
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public bool IsPublic { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private AppConfiguration() { }

    public static AppConfiguration Create(
        string key,
        string value,
        string category,
        bool isPublic)
    {
        return new AppConfiguration
        {
            Id = Guid.NewGuid(),
            Key = key,
            Value = value,
            Category = category,
            IsPublic = isPublic,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateValue(string value)
    {
        Value = value;
        UpdatedAt = DateTime.UtcNow;
    }
}
