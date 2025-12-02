using Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Files;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<LocalFileStorage> _logger;
    private readonly string _baseUrl;

    public LocalFileStorage(IWebHostEnvironment env, IConfiguration cfg, ILogger<LocalFileStorage> logger)
    {
        _env = env;
        _logger = logger;
        // expose via /uploads path by default; allow override via config
        _baseUrl = cfg["Uploads:BaseUrl"]?.TrimEnd('/') ?? "/uploads";
    }

    public async Task<string> SaveMenuItemImageAsync(Guid menuItemId, Stream content, string contentType, string fileName, string kind, CancellationToken ct = default)
    {
        var safeKind = kind is "avatar" or "background" ? kind : "other";
        var ext = Path.GetExtension(fileName);
        var webRoot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var dir = Path.Combine(webRoot, "uploads", "menu-items", menuItemId.ToString(), safeKind);
        Directory.CreateDirectory(dir);
        var name = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}{ext}";
        var fullPath = Path.Combine(dir, name);
        await using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(fs, ct);
        }
        var relUrl = CombineBaseUrl($"menu-items/{menuItemId}/{safeKind}/{name}");
        return relUrl;
    }

    public async Task<string> SaveTempImageAsync(Stream content, string contentType, string fileName, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(fileName);
        var webRoot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var dir = Path.Combine(webRoot, "uploads", "temp-images");
        Directory.CreateDirectory(dir);
        var name = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(dir, name);
        await using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(fs, ct);
        }
        var relUrl = CombineBaseUrl($"temp-images/{name}");
        return relUrl;
    }

    public async Task<string> SaveTempImageForSkuAsync(string sku, Stream content, string contentType, string fileName, string nameSuffix = "image", CancellationToken ct = default)
    {
        // sanitize sku and suffix to be file-system friendly
        string Sanitize(string s)
        => string.Concat(s.Where(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_'));

        var safeSku = string.IsNullOrWhiteSpace(sku) ? "unknown" : Sanitize(sku.Trim());
        var safeSuffix = string.IsNullOrWhiteSpace(nameSuffix) ? "image" : Sanitize(nameSuffix.Trim());
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext))
        {
            // best-effort default by MIME
            ext = contentType switch
            {
                "image/png" => ".png",
                "image/jpeg" or "image/jpg" => ".jpg",
                "image/webp" => ".webp",
                "image/gif" => ".gif",
                _ => ".bin"
            };
        }

        var webRoot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var dir = Path.Combine(webRoot, "uploads", "temp-images", safeSku);
        Directory.CreateDirectory(dir);
        var name = $"{safeSku}-{safeSuffix}{ext}";
        var fullPath = Path.Combine(dir, name);
        await using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(fs, ct);
        }
        var relUrl = CombineBaseUrl($"temp-images/{safeSku}/{name}");
        return relUrl;
    }

    public Task<bool> DeleteByUrlAsync(string url, CancellationToken ct = default)
    {
        try
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");

            // Support both absolute and relative URLs
            var normalized = url.Trim();
            string relPath;
            if (normalized.Contains("://"))
            {
                // absolute: find "/uploads/" segment and take what follows
                var idx = normalized.IndexOf("/uploads/", StringComparison.OrdinalIgnoreCase);
                if (idx < 0) return Task.FromResult(false);
                relPath = normalized[(idx + "/uploads/".Length)..];
            }
            else
            {
                // relative: may start with configured base or "/uploads"
                var prefix = _baseUrl.TrimEnd('/') + "/";
                if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    relPath = normalized[prefix.Length..];
                }
                else if (normalized.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                {
                    relPath = normalized["/uploads/".Length..];
                }
                else
                {
                    // If it is already a relative uploads path
                    relPath = normalized.TrimStart('/');
                }
            }

            var path = Path.Combine(webRoot, relPath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(path))
            {
                File.Delete(path);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Delete file failed for {Url}", url);
            return Task.FromResult(false);
        }
    }

    private string CombineBaseUrl(string relative)
    {
        // _baseUrl can be absolute (http://host/uploads) or relative (/uploads)
        return _baseUrl + "/" + relative.TrimStart('/');
    }
}
