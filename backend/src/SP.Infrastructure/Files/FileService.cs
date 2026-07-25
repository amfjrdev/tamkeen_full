using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SP.Application.Abstractions.Files;

namespace SP.Infrastructure.Files;

internal sealed class FileService : IFileService
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    private readonly long _maxFileSizeBytes;
    private readonly string _basePath;
    private readonly string _baseUrl;
    private readonly ILogger<FileService> _logger;

    public FileService(IConfiguration configuration, ILogger<FileService> logger)
    {
        _basePath = configuration["FileStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _baseUrl = configuration["FileStorage:BaseUrl"] ?? "/uploads";
        _maxFileSizeBytes = configuration.GetValue<long>("FileStorage:MaxFileSizeBytes", 5 * 1024 * 1024);
        _logger = logger;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<List<string>> UploadFilesAsync(
        List<IFormFile> files, string folder,
        CancellationToken cancellationToken = default)
    {
        var urls = new List<string>();
        foreach (var file in files)
        {
            var url = await UploadSingleAsync(file, folder, cancellationToken);
            if (url is not null) urls.Add(url);
        }
        return urls;
    }

    public Task DeleteFilesAsync(List<string> fileUrls, CancellationToken cancellationToken = default)
    {
        foreach (var url in fileUrls)
        {
            try
            {
                // Convert URL back to physical path
                var relativePath = url.Replace(_baseUrl, string.Empty).TrimStart('/');
                var fullPath = Path.Combine(_basePath, relativePath);
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file: {Url}", url);
            }
        }
        return Task.CompletedTask;
    }

    public async Task<string> UploadAsync(
        string fileName, Stream fileContent, string contentType,
        CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"File type '{ext}' is not allowed.");

        var safeFileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(_basePath, safeFileName);

        await using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await fileContent.CopyToAsync(fs, cancellationToken);

        return $"{_baseUrl}/{safeFileName}";
    }

    private async Task<string?> UploadSingleAsync(
        IFormFile file, string folder, CancellationToken cancellationToken)
    {
        if (file.Length == 0 || file.Length > _maxFileSizeBytes)
        {
            _logger.LogWarning("Rejected file {Name}: size {Size}", file.FileName, file.Length);
            return null;
        }

        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(ext))
        {
            _logger.LogWarning("Rejected file {Name}: extension {Ext}", file.FileName, ext);
            return null;
        }

        var folderPath = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(folderPath);

        var safeFileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(folderPath, safeFileName);

        await using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await file.CopyToAsync(stream, cancellationToken);

        return $"{_baseUrl}/{folder}/{safeFileName}";
    }
}
