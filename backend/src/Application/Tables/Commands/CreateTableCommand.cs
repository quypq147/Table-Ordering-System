using Application.Abstractions;
using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tables.Commands
{
    public sealed record CreateTableCommand(string Id, string Code, int Seats) : ICommand<TableDto>;
}
