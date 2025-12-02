using Application.Abstractions;
using Application.Dtos;

namespace Application.MenuItems.Commands
{
    public sealed record ChangeMenuItemPriceCommand(Guid Id, decimal Price, string Currency) : ICommand<MenuItemDto>;
}

