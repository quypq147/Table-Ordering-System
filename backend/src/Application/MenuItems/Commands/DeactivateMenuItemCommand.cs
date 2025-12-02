using Application.Abstractions;
using Application.Dtos;

namespace Application.MenuItems.Commands
{
    public sealed record DeactivateMenuItemCommand(Guid Id) : ICommand<MenuItemDto>;
}
