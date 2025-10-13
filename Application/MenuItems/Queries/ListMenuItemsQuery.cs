using Application.Abstractions;
using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.MenuItems.Queries
{
    public sealed record ListMenuItemsQuery(string? Search = null, bool OnlyActive = true, int Page = 1, int PageSize = 20)
    : IQuery<IReadOnlyList<MenuItemDto>>;
}
