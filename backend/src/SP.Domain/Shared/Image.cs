namespace SP.Domain.Shared;

public sealed record Image(string Url, bool IsMain = false)
{
    public static readonly Image Default = new("https://ui-avatars.com/api/?background=random", true);

    public static Image Create(string url, bool isMain = false)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Image URL cannot be null or empty.");

        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) && !url.StartsWith("/"))
            throw new ArgumentException("Invalid image URL.");

        return new Image(url, isMain);
    }
}