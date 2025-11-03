// Get by Id
using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

public sealed record GetTableByIdQuery(Guid Id) : IQuery<TableDto?>;

public sealed class GetTableByIdHandler
    : IQueryHandler<GetTableByIdQuery, TableDto?>
{
    private readonly IApplicationDbContext _db;
    public GetTableByIdHandler(IApplicationDbContext db) => _db = db;

    public async Task<TableDto?> Handle(GetTableByIdQuery q, CancellationToken ct)
    {
        var t = await _db.Tables.AsNoTracking().FirstOrDefaultAsync(x => x.Id == q.Id, ct);
        return t is null ? null : TableMapper.ToDto(t);
    }
}

// List by Status
public sealed record ListTablesByStatusQuery(Domain.Enums.TableStatus Status)
    : IQuery<IReadOnlyList<TableDto>>;

public sealed class ListTablesByStatusHandler
    : IQueryHandler<ListTablesByStatusQuery, IReadOnlyList<TableDto>>
{
    private readonly IApplicationDbContext _db;
    public ListTablesByStatusHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<TableDto>> Handle(ListTablesByStatusQuery q, CancellationToken ct)
    {
        var rows = await _db.Tables.AsNoTracking()
            .Where(t => t.Status == q.Status)
            .OrderBy(t => t.Code)
            .ToListAsync(ct);

        return rows.Select(TableMapper.ToDto).ToList();
    }
}

