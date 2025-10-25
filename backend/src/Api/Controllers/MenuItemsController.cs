// Api/Controllers/MenuItemsController.cs
using Application.Abstractions;                // ISender, IQuery/ICommand
using Application.Common.CQRS;
using Application.Dtos;                        // MenuItemDto
using Application.MenuItems.Commands;          // CreateMenuItemCommand, ChangeMenuItemPriceCommand, DeactivateMenuItemCommand
using Application.MenuItems.Queries;           // ListMenuItemsQuery
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuItemsController : ControllerBase
{
    private readonly ISender _sender;
    public MenuItemsController(ISender sender) => _sender = sender;

    // GET /api/menuitems?search=tra&page=1&pageSize=20&onlyActive=true
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MenuItemDto>>> List(
        [FromQuery] string? search,
        [FromQuery] bool onlyActive = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _sender.Send(new ListMenuItemsQuery(search, onlyActive, page, pageSize));
        return Ok(result);
    }

    public sealed record CreateDto(string Id, string Name, decimal Price, string Currency);

    // POST /api/menuitems
    [HttpPost]
    public async Task<ActionResult<MenuItemDto>> Create([FromBody] CreateDto body)
    {
        var dto = await _sender.Send(new CreateMenuItemCommand(body.Id, body.Name, body.Price, body.Currency));
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    // GET /api/menuitems/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<MenuItemDto>> GetById(string id)
    {
        var dto = await _sender.Send(new GetMenuItemByIdQuery(id));
        return dto is null ? NotFound() : Ok(dto);
    }

    public sealed record ChangePriceDto(decimal Price, string Currency);

    // PATCH /api/menuitems/{id}/price
    [HttpPatch("{id}/price")]
    public async Task<ActionResult<MenuItemDto>> ChangePrice(string id, [FromBody] ChangePriceDto body)
    {
        var dto = await _sender.Send(new ChangeMenuItemPriceCommand(id, body.Price, body.Currency));
        return Ok(dto);
    }

    // POST /api/menuitems/{id}/deactivate
    [HttpPost("{id}/deactivate")]
    public async Task<ActionResult<MenuItemDto>> Deactivate(string id)
        => Ok(await _sender.Send(new DeactivateMenuItemCommand(id)));
}

