namespace Application.Abstractions;

public interface IFileStorage
{
    Task<string> SaveMenuItemImageAsync(
    Guid menuItemId,
    Stream content,
    string contentType,
    string fileName,
    string kind, // "avatar" | "background"
    CancellationToken ct = default);

    Task<bool> DeleteByUrlAsync(string url, CancellationToken ct = default);

    // New: generic temp image save for compatibility uploads (/api/uploads/images)
    Task<string> SaveTempImageAsync(
    Stream content,
    string contentType,
    string fileName,
    CancellationToken ct = default);

    // New: save temp image under a SKU folder with a deterministic file name
    // Resulting path: /uploads/temp-images/{sku}/{sku}-{nameSuffix}{extension}
    Task<string> SaveTempImageForSkuAsync(
    string sku,
    Stream content,
    string contentType,
    string fileName,
    string nameSuffix = "image",
    CancellationToken ct = default);
}
