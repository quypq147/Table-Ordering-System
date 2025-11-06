using Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
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
 var relUrl = $"{_baseUrl}/menu-items/{menuItemId}/{safeKind}/{name}";
 return relUrl;
 }

 public Task<bool> DeleteByUrlAsync(string url, CancellationToken ct = default)
 {
 try
 {
 var webRoot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
 var prefix = _baseUrl.TrimEnd('/') + "/";
 if (!url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return Task.FromResult(false);
 var rel = url.Substring(prefix.Length).Replace('/', Path.DirectorySeparatorChar);
 var path = Path.Combine(webRoot, rel);
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
}
