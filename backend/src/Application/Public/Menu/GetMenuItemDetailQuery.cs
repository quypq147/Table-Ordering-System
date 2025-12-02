using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Public.Menu;

public sealed record GetMenuItemDetailQuery(Guid Id) : IRequest<MenuItemDto?>;

public sealed class GetMenuItemDetailHandler
    : IRequestHandler<GetMenuItemDetailQuery, MenuItemDto?>
{
    private readonly IApplicationDbContext _db;
    public GetMenuItemDetailHandler(IApplicationDbContext db) => _db = db;

    public async Task<MenuItemDto?> Handle(GetMenuItemDetailQuery q, CancellationToken ct)
    {
        var entity = await _db.MenuItems.FirstOrDefaultAsync(x => x.Id == q.Id && x.IsActive, ct);
        return entity is null ? null : MenuItemMapper.ToDto(entity);
    }
}