﻿using Application.Abstractions;
using Application.Dtos;

namespace Application.MenuItems.Commands
{
    public sealed record RenameMenuItemCommand(string Id, string NewName) : ICommand<MenuItemDto>;
}
