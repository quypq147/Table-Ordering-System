using Application.Abstractions;   // IQuery<T>
using Application.Dtos;          // MenuItemDto

namespace Application.MenuItems.Queries;

public sealed record GetMenuItemByIdQuery(Guid Id) : IQuery<MenuItemDto?>;
