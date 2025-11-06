using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
}
