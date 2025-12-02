using Application.Abstractions;
using Application.Dtos;

namespace Application.Tables.Queries
{
    public sealed record ListTablesByStatusQuery(string Status) : IQuery<IReadOnlyList<TableDto>>;
}
