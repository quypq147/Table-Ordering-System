// Api/Controllers/MenuItemsController.cs
using Application.Common.CQRS;
using Application.Dtos;                        // MenuItemDto
using Application.MenuItems.Commands;          // CreateMenuItemCommand, ChangeMenuItemPriceCommand, DeactivateMenuItemCommand, ActivateMenuItemCommand, RenameMenuItemCommand
using Application.MenuItems.Queries;           // ListMenuItemsQuery, GetMenuItemByIdQuery
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuItemsController : ControllerBase
{
    private readonly ISender _sender;
    public MenuItemsController(ISender sender) => _sender = sender;

    public sealed record CreateDto(
        [Required] Guid CategoryId,
        [Required, StringLength(200, MinimumLength = 1)] string Name,
        [Required, StringLength(64, MinimumLength = 1)] string Sku, // NEW
        [Range(0.01, 1_000_000)] decimal Price,
        [Required, StringLength(3, MinimumLength = 3)] string Currency,
        [StringLength(1024)] string? AvatarImageUrl,
        [StringLength(1024)] string? BackgroundImageUrl
    );

    public sealed record RenameDto([property: Required, StringLength(200, MinimumLength = 1)] string NewName);
    public sealed record PriceDto([property: Range(0.01, 1_000_000)] decimal Price,
        [property: Required, StringLength(3, MinimumLength = 3)] string Currency);

    // multipart form for image uploads
    public sealed record UpdateImagesResponse(Guid Id, string? AvatarImageUrl, string? BackgroundImageUrl);

    // POST /api/menuitems
    [HttpPost]
    public async Task<ActionResult<MenuItemDto>> Create([FromBody] CreateDto body, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var cmd = new CreateMenuItemCommand(
            body.CategoryId,
            body.Name.Trim(),
            body.Sku.Trim(),
            body.Price,
            body.Currency.Trim().ToUpperInvariant(),
            body.AvatarImageUrl?.Trim(),
            body.BackgroundImageUrl?.Trim()
        );

        var dto = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    // Helper to pick files from common field names
    private static (IFormFile? avatar, IFormFile? background) GetImageFilesFromForm(IFormCollection form)
    {
        IFormFile? Pick(params string[] names)
        {
            foreach (var n in names)
            {
                var f = form.Files.GetFile(n);
                if (f is not null) return f;
            }
            return null;
        }

        var avatar = Pick("avatar", "image", "file", "files[0]", "files");
        var background = Pick("background", "bg", "image2", "files[1]");

        if (avatar is null && form.Files.Count > 0) avatar = form.Files[0];
        if (background is null && form.Files.Count > 1) background = form.Files[1];

        return (avatar, background);
    }

    // PUT /api/menuitems/{id}/images
    [HttpPut("{id:guid}/images")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = 64 * 1024 * 1024)]
    public async Task<ActionResult<UpdateImagesResponse>> UpdateImages([FromRoute] Guid id, CancellationToken ct)
    {
        var form = await Request.ReadFormAsync(ct);
        var (avatarFile, bgFile) = GetImageFilesFromForm(form);

        Stream? avatarStream = avatarFile?.OpenReadStream();
        Stream? bgStream = bgFile?.OpenReadStream();

        if (avatarStream is null && bgStream is null)
            return BadRequest("Không tìm thấy tệp ảnh trong form-data");

        var cmd = new UpdateMenuItemImagesCommand(
            id,
            avatarStream,
            avatarFile?.FileName,
            avatarFile?.ContentType,
            bgStream,
            bgFile?.FileName,
            bgFile?.ContentType
        );
        var dto = await _sender.Send(cmd, ct);
        return Ok(new UpdateImagesResponse(dto.Id, dto.AvatarImageUrl, dto.BackgroundImageUrl));
    }

    // Some uploaders only support POST; map POST to the same handler
    [HttpPost("{id:guid}/images")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = 64 * 1024 * 1024)]
    public Task<ActionResult<UpdateImagesResponse>> UploadImages([FromRoute] Guid id, CancellationToken ct)
        => UpdateImages(id, ct);

    // GET /api/menuitems/{id}
    [HttpGet("{id:guid}")]
    public Task<MenuItemDto?> GetById(Guid id, CancellationToken ct)
        => _sender.Send(new GetMenuItemByIdQuery(id), ct);

    // GET /api/menuitems?search=tra&page=1&pageSize=20&onlyActive=true
    [HttpGet]
    public async Task<List<MenuItemDto>> List(
        [FromQuery] string? search,
        [FromQuery] Guid? categoryId,
        CancellationToken ct,
        [FromQuery] bool onlyActive = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _sender.Send(new ListMenuItemsQuery
        {
            Search = search,
            CategoryId = categoryId,
            OnlyActive = onlyActive,
            Page = page,
            PageSize = pageSize
        }, ct);

        return result.ToList();
    }

    // PUT /api/menuitems/{id}/rename
    [HttpPut("{id:guid}/rename")]
    public async Task<ActionResult<MenuItemDto>> Rename(Guid id, [FromBody] RenameDto body, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var dto = await _sender.Send(new RenameMenuItemCommand(id, body.NewName.Trim()), ct);
        return Ok(dto);
    }

    // PUT /api/menuitems/{id}/price
    [HttpPut("{id:guid}/price")]
    public async Task<ActionResult<MenuItemDto>> ChangePrice(Guid id, [FromBody] PriceDto body, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var dto = await _sender.Send(new ChangeMenuItemPriceCommand(id, body.Price, body.Currency.Trim().ToUpperInvariant()), ct);
        return Ok(dto);
    }

    // POST /api/menuitems/{id}/activate
    [HttpPost("{id:guid}/activate")]
    public Task<MenuItemDto> Activate(Guid id, CancellationToken ct)
        => _sender.Send(new ActivateMenuItemCommand(id), ct);

    // POST /api/menuitems/{id}/deactivate
    [HttpPost("{id:guid}/deactivate")]
    public Task<MenuItemDto> Deactivate(Guid id, CancellationToken ct)
        => _sender.Send(new DeactivateMenuItemCommand(id), ct);

    // DELETE /api/menuitems/{id}
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeactivateMenuItemCommand(id), ct);
        return NoContent();
    }
}

