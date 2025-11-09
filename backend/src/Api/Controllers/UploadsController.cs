using Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Provides compatibility endpoints for clients that expect /api/uploads/* routes.
[ApiController]
[Route("api/uploads")]
public class UploadsController : ControllerBase
{
    private readonly IFileStorage _files;
    public UploadsController(IFileStorage files) => _files = files;

    public sealed record UploadResult(string url);

    // POST /api/uploads/images
    // Also handle common variants via additional routes.
    [HttpPost("images")]
    [HttpPost("image")]
    [HttpPost("../upload/images")]
    [HttpPost("../upload/image")]
    [HttpPost("../files/images")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = 64 * 1024 * 1024)]
    public async Task<ActionResult<UploadResult>> UploadImage(CancellationToken ct)
    {
        var form = await Request.ReadFormAsync(ct);
        IFormFile? file = form.Files.Count > 0 ? form.Files[0] : null;
        if (file is null)
        {
            // try common keys
            file = form.Files.GetFile("file")
            ?? form.Files.GetFile("image")
            ?? form.Files.GetFile("files")
            ?? form.Files.GetFile("files[0]");
        }
        if (file is null) return BadRequest("Không tìm thấy tệp ảnh trong form-data");

        // Optional SKU-based path: /uploads/temp-images/{sku}/{sku}-{suffix}.ext
        var sku = form["sku"].FirstOrDefault();
        var suffix = form["suffix"].FirstOrDefault()
        ?? form["name"].FirstOrDefault()
        ?? form["type"].FirstOrDefault()
        ?? form["kind"].FirstOrDefault()
        ?? "avatar"; // default suffix

        await using var stream = file.OpenReadStream();
        string url;
        if (!string.IsNullOrWhiteSpace(sku))
        {
            url = await _files.SaveTempImageForSkuAsync(sku!, stream, file.ContentType, file.FileName, suffix, ct);
        }
        else
        {
            url = await _files.SaveTempImageAsync(stream, file.ContentType, file.FileName, ct);
        }
        return Ok(new UploadResult(url));
    }
}
