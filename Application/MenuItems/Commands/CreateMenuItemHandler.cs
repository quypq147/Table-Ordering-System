using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Application.MenuItems.Commands
{
    public sealed class CreateMenuItemHandler : ICommandHandler<CreateMenuItemCommand, MenuItemDto>
    {
        private readonly IApplicationDbContext _db;
        public CreateMenuItemHandler(IApplicationDbContext db) => _db = db;

        public async Task<MenuItemDto> Handle(CreateMenuItemCommand c, CancellationToken ct)
        {
            var exists = await _db.MenuItems.AnyAsync(x => x.Id == c.Id, ct);
            if (exists) throw new InvalidOperationException("Món ăn đã trong thực đơn.");

            var m = new MenuItem(c.Id, c.Name, new Money(c.Price, c.Currency)); // ctor + rule rename/price :contentReference[oaicite:12]{index=12}
            _db.MenuItems.Add(m);
            await _db.SaveChangesAsync(ct);
            return MenuItemMapper.ToDto(m);
        }
    }
}
