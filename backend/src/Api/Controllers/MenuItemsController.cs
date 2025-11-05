// Api/Controllers/MenuItemsController.cs
using Application.Abstractions;                // ISender, IQuery/ICommand
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
        [Required, StringLength(3, MinimumLength = 3)] string Currency
    );

    public sealed record RenameDto([property: Required, StringLength(200, MinimumLength = 1)] string NewName);
    public sealed record PriceDto([property: Range(0.01, 1_000_000)] decimal Price,
        [property: Required, StringLength(3, MinimumLength = 3)] string Currency);

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
            body.Currency.Trim().ToUpperInvariant()
        );

        var dto = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

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
}

