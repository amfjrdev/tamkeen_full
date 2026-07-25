using Microsoft.Extensions.Configuration;
using SP.Application.Abstractions;

namespace SP.Infrastructure;

internal sealed class AppSettings : IAppSettings
{
    public string BaseUrl { get; }
    public string DefaultAvatarUrl { get; }

    public AppSettings(IConfiguration configuration)
    {
        BaseUrl = configuration["App:BaseUrl"]
            ?? throw new InvalidOperationException("App:BaseUrl is not configured.");
        DefaultAvatarUrl = configuration["App:DefaultAvatarUrl"]
            ?? throw new InvalidOperationException("App:DefaultAvatarUrl is not configured.");
    }
}
