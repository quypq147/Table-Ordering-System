using Application.Abstractions;
using Application.Dtos;

namespace Application.MenuItems.Commands;

public sealed record CreateMenuItemCommand(
    Guid CategoryId,
    string Name,
    string Sku, // accept Sku
    decimal Price,
    string Currency,
    string? AvatarImageUrl = null,
    string? BackgroundImageUrl = null
) : ICommand<MenuItemDto>;
