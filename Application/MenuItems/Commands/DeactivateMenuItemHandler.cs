using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.MenuItems.Commands
{
    public sealed class DeactivateMenuItemHandler : ICommandHandler<DeactivateMenuItemCommand, MenuItemDto>
    {
        private readonly IApplicationDbContext _db;
        public DeactivateMenuItemHandler(IApplicationDbContext db) => _db = db;

        public async Task<MenuItemDto> Handle(DeactivateMenuItemCommand c, CancellationToken ct)
        {
            var m = await _db.MenuItems.FirstOrDefaultAsync(x => x.Id == c.Id, ct)
                    ?? throw new KeyNotFoundException("Menu item not found");
            m.Deactivate(); // :contentReference[oaicite:16]{index=16}
            await _db.SaveChangesAsync(ct);
            return MenuItemMapper.ToDto(m);
        }
    }
}
