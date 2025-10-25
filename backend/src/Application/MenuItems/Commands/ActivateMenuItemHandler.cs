using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.MenuItems.Commands
{
    public sealed class ActivateMenuItemHandler : ICommandHandler<ActivateMenuItemCommand, MenuItemDto>
    {
        private readonly IApplicationDbContext _db;
        public ActivateMenuItemHandler(IApplicationDbContext db) => _db = db;

        public async Task<MenuItemDto> Handle(ActivateMenuItemCommand c, CancellationToken ct)
        {
            var m = await _db.MenuItems.FirstOrDefaultAsync(x => x.Id == c.Id, ct)
                    ?? throw new KeyNotFoundException("Menu item not found");
            m.Activate(); // :contentReference[oaicite:15]{index=15}
            await _db.SaveChangesAsync(ct);
            return MenuItemMapper.ToDto(m);
        }
    }
}
