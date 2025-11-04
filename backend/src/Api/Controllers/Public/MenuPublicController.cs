using Application.Dtos;
using Application.Public.Categories;
using Application.Public.Menu;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Public;

[ApiController]
[Route("api/public/menu")]
public class MenuPublicController : ControllerBase
{
    private readonly ISender _sender;
    public MenuPublicController(ISender sender) => _sender = sender;

    // GET /api/public/menu/categories
    [HttpGet("categories")]
    public Task<IReadOnlyList<CategoryDto>> Categories(CancellationToken ct)
        => _sender.Send(new ListPublicCategoriesQuery(), ct);

    // GET /api/public/menu/by-category/{categoryId}
    [HttpGet("by-category/{categoryId:guid}")]
    public Task<IReadOnlyList<MenuItemDto>> ByCategory(Guid categoryId, CancellationToken ct)
        => _sender.Send(new ListMenuByCategoryQuery(categoryId), ct);

    // GET /api/public/menu/items/{id}
    [HttpGet("items/{id:guid}")]
    public Task<MenuItemDto?> Item(Guid id, CancellationToken ct)
        => _sender.Send(new GetMenuItemDetailQuery(id), ct);
}