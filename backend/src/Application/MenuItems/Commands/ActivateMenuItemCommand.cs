using Application.Abstractions;
using Application.Dtos;

namespace Application.MenuItems.Commands
{
    public sealed record ActivateMenuItemCommand(Guid Id) : ICommand<MenuItemDto>;
}
