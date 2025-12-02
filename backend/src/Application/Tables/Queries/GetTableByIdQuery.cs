using Application.Abstractions;
using Application.Dtos;

namespace Application.Tables.Queries
{
    public sealed record GetTableByIdQuery(string Id) : IQuery<TableDto?>;
}
