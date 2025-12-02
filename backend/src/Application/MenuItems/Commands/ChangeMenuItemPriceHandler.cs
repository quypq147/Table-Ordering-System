using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Application.MenuItems.Commands
{
    public sealed class ChangeMenuItemPriceHandler : ICommandHandler<ChangeMenuItemPriceCommand, MenuItemDto>
    {
        private readonly IApplicationDbContext _db;
        public ChangeMenuItemPriceHandler(IApplicationDbContext db) => _db = db;

        public async Task<MenuItemDto> Handle(ChangeMenuItemPriceCommand c, CancellationToken ct)
        {
            var m = await _db.MenuItems.FirstOrDefaultAsync(x => x.Id == c.Id, ct)
                    ?? throw new KeyNotFoundException("Món ăn không có trong menu");

            m.ChangePrice(new Money(c.Price, c.Currency)); // :contentReference[oaicite:13]{index=13}
            await _db.SaveChangesAsync(ct);
            return MenuItemMapper.ToDto(m);
        }
    }
}
