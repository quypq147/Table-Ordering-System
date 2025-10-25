using Application.Abstractions;   // IQuery<T>
using Application.Dtos;          // MenuItemDto

namespace Application.MenuItems.Queries;

public sealed record GetMenuItemByIdQuery(string Id) : IQuery<MenuItemDto?>;
