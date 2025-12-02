// Application/Tables/Queries/GetAllTablesHandler.cs
using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.Tables.Queries;

public sealed class GetAllTablesHandler
    : IQueryHandler<GetAllTablesQuery, IReadOnlyList<TableDto>>
{
    private readonly IApplicationDbContext _db; // namespace theo file IApplicationDbContext của bạn
    public GetAllTablesHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<TableDto>> Handle(GetAllTablesQuery q, CancellationToken ct)
    {
        var rows = await _db.Tables.AsNoTracking().OrderBy(t => t.Code).ToListAsync(ct);
        return rows.Select(TableMapper.ToDto).ToList();
    }
}

