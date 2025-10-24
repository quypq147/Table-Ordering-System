// Application/Tables/Queries/GetAllTablesQuery.cs
using Application.Abstractions;
using Application.Common.CQRS;
using Application.Dtos;
using System.Collections.Generic;

namespace Application.Tables.Queries;

public sealed record GetAllTablesQuery() : IQuery<IReadOnlyList<TableDto>>;

