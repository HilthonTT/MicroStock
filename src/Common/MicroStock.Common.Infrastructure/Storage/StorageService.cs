using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MicroStock.Common.Application.Extensions;
using MicroStock.Common.Application.Origin;
using MicroStock.Common.Application.Storage;
using System.Text.RegularExpressions;
using System.Web;

namespace MicroStock.Common.Infrastructure.Storage;

internal partial class StorageService(
    IOptions<OriginOptions> options, 
    IWebHostEnvironment environment,
    ILogger<StorageService> logger) : IStorageService
{
    private readonly string _basePath = Path.Combine(environment.ContentRootPath, "wwwroot");
    private readonly OriginOptions _originOptions = options.Value;

    public async Task<Uri> UploadAsync<T>(
        string name, 
        string extension, 
        string data, 
        FileType supportedFileType, 
        CancellationToken cancellationToken = default)
    {
        extension = extension.StartsWith('.') ? extension : $".{extension}";
        extension = extension.ToLowerInvariant();

        if (!supportedFileType.GetDescription().Contains(extension))
        {
            throw new InvalidOperationException($"File format '{extension}' is not supported.");
        }

        if (!TryExtractBase64Data(data, out var base64Data))
        {
            throw new InvalidOperationException("Invalid base64 data URL.");
        }

        await using var stream = new MemoryStream(Convert.FromBase64String(base64Data));

        if (stream.Length == 0)
        {
            throw new InvalidOperationException("Empty file uploaded.");
        }

        string folder = typeof(T).Name;
        var relativeFolder = supportedFileType switch
        {
            FileType.Image => Path.Combine("assets", "images", folder),
            _ => Path.Combine("assets", "others", folder)
        };

        string fullDirectory = Path.Combine(_basePath, relativeFolder);
        if (!Directory.Exists(fullDirectory))
        {
            Directory.CreateDirectory(fullDirectory);
        }

        string safeFileName = SanitizeFileName(name);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(safeFileName);
        string finalFileName = fileNameWithoutExtension + extension;

        string relativePath = Path.Combine(relativeFolder, finalFileName).Replace('\\', '/');
        string fullPath = Path.Combine(fullDirectory, finalFileName);

        string uniquePath = GetUniqueFilePath(fullPath);
        relativePath = Path.GetRelativePath(_basePath, uniquePath).Replace('\\', '/');

        await using var fileStream = new FileStream(uniquePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fileStream, cancellationToken);

        return new Uri(_originOptions.OriginUrl, relativePath);
    }

    public void Remove(Uri? path)
    {
        if (path is null || !path.IsAbsoluteUri)
        {
            return;
        }

        string relativePath = GetRelativePathFromUri(path);
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        string fullPath = Path.Combine(_basePath, relativePath.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to remove the stored data");
            }
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "file";
        }

        fileName = Path.GetFileName(fileName);

        // Replace spaces and special chars with hyphens, keep letters, numbers, dots, hyphens, underscores
        string sanitized = InvalidFileNameCharsRegex().Replace(fileName, "-");

        // Remove consecutive hyphens
        sanitized = ConsecutiveHyphensRegex().Replace(sanitized, "-");

        // Trim hyphens from start/end
        sanitized = sanitized.Trim('-', '.');

        // Fallback if empty after sanitization
        return string.IsNullOrWhiteSpace(sanitized) ? "file" : sanitized;
    }

    private string GetRelativePathFromUri(Uri uri)
    {
        if (!uri.AbsoluteUri.StartsWith(_originOptions.OriginUrl.AbsoluteUri, StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        string relative = uri.AbsolutePath.TrimStart('/');

        return HttpUtility.UrlDecode(relative);
    }
    
    private static bool TryExtractBase64Data(string dataUrl, out string base64Data)
    {
        Match match = DataUrlRegex().Match(dataUrl);
        if (match.Success)
        {
            base64Data = match.Groups["data"].Value;
            return true;
        }
        base64Data = string.Empty;
        return false;
    }

    private static string GetUniqueFilePath(string desiredPath)
    {
        if (!File.Exists(desiredPath))
        {
            return desiredPath;
        }

        string directory = Path.GetDirectoryName(desiredPath)!;
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(desiredPath);
        string extension = Path.GetExtension(desiredPath);

        int counter = 1;
        string newFilePath;
        do
        {
            string newFileName = $"{fileNameWithoutExtension}-{counter++}{extension}";
            newFilePath = Path.Combine(directory, newFileName);
        } while (File.Exists(newFilePath));

        return newFilePath;
    }

    [GeneratedRegex(@"^data:\w+/\w+;base64,(?<data>.+)$")]
    private static partial Regex DataUrlRegex();

    [GeneratedRegex(@"[^a-zA-Z0-9._-]+")]
    private static partial Regex InvalidFileNameCharsRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex ConsecutiveHyphensRegex();
}
