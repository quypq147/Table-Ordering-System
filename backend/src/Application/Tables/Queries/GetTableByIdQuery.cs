using Application.Abstractions;
using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tables.Queries
{
    public sealed record GetTableByIdQuery(string Id) : IQuery<TableDto?>;
}
