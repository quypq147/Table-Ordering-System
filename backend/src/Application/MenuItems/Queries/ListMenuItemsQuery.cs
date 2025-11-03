using Application.Abstractions;
using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.MenuItems.Queries
{
    public sealed record ListMenuItemsQuery : IQuery<IReadOnlyList<MenuItemDto>>
    {
        public string? Search { get; init; }
        public Guid? CategoryId { get; init; }
        public bool OnlyActive { get; init; } = true;
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}
