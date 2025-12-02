// Application/Tables/Queries/GetAllTablesQuery.cs
using Application.Abstractions;
using Application.Dtos;

namespace Application.Tables.Queries;

public sealed record GetAllTablesQuery() : IQuery<IReadOnlyList<TableDto>>;

