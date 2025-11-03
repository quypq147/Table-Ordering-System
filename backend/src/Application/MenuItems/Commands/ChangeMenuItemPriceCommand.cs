using Application.Abstractions;
using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.MenuItems.Commands
{
    public sealed record ChangeMenuItemPriceCommand(Guid Id, decimal Price, string Currency) : ICommand<MenuItemDto>;
}

