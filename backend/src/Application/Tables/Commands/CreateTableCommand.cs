using Application.Abstractions;
using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tables.Commands
{
    public sealed record CreateTableCommand(Guid Id, string Code, int Seats) : ICommand<TableDto>;
}
