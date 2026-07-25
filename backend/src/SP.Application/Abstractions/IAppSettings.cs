namespace SP.Application.Abstractions;

public interface IAppSettings
{
    string BaseUrl { get; }
    string DefaultAvatarUrl { get; }
}
