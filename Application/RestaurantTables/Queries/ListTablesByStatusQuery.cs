using Application.Abstractions;
using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RestaurantTables.Queries
{
    public sealed record ListTablesByStatusQuery(string Status) : IQuery<IReadOnlyList<TableDto>>;
}
