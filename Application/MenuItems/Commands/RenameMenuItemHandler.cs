using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Application.MenuItems.Commands;
using Microsoft.EntityFrameworkCore;

namespace Application.MenuItems.Commaclnds
{
    public sealed class RenameMenuItemHandler : ICommandHandler<RenameMenuItemCommand, MenuItemDto>
    {
        private readonly IApplicationDbContext _db;
        public RenameMenuItemHandler(IApplicationDbContext db) => _db = db;

        public async Task<MenuItemDto> Handle(RenameMenuItemCommand c, CancellationToken ct)
        {
            var m = await _db.MenuItems.FirstOrDefaultAsync(x => x.Id == c.Id, ct)
                    ?? throw new KeyNotFoundException("Menu item not found");

            m.Rename(c.NewName); // null/whitespace check trong Domain :contentReference[oaicite:14]{index=14}
            await _db.SaveChangesAsync(ct);
            return MenuItemMapper.ToDto(m);
        }
    }
}
