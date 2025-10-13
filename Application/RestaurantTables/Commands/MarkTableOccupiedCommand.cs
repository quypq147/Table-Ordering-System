using Application.Abstractions;
using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RestaurantTables.Commands
{
    public sealed record MarkTableOccupiedCommand(string Id) : ICommand<TableDto>;
}
