using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Application.MenuItems.Commands;

public sealed class CreateMenuItemHandler : ICommandHandler<CreateMenuItemCommand, MenuItemDto>
{
    private readonly IApplicationDbContext _db;
    public CreateMenuItemHandler(IApplicationDbContext db) => _db = db;

    public async Task<MenuItemDto> Handle(CreateMenuItemCommand c, CancellationToken ct)
    {
        //1) Ensure Category exists
        var categoryExists = await _db.Categories.AnyAsync(x => x.Id == c.CategoryId, ct);
        if (!categoryExists) throw new InvalidOperationException("Category not found");

        //2) Normalize and check SKU uniqueness
        var sku = c.Sku.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(sku)) throw new InvalidOperationException("SKU is required");

        var skuExists = await _db.MenuItems.AnyAsync(x => x.Sku == sku, ct);
        if (skuExists) throw new InvalidOperationException("SKU nay da co roi");

        //3) Normalize other fields
        var name = c.Name.Trim();
        var currency = c.Currency.Trim().ToUpperInvariant();
        if (currency.Length != 3) throw new InvalidOperationException("Invalid currency");

        //4) Create entity and assign SKU + Category
        var item = new MenuItem(
            id: Guid.NewGuid(),
            categoryId: c.CategoryId,
            name: name,
            sku: sku,
            price: new Money(c.Price, currency)
        );
        item.Activate();
        item.SetAvatarImage(c.AvatarImageUrl);
        item.SetBackgroundImage(c.BackgroundImageUrl);

        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync(ct);

        //5) Map to DTO
        return MenuItemMapper.ToDto(item);
    }
}
