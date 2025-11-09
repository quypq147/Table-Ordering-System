using Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Public.Tables;

public sealed record GetTableByCodeQuery(string Code) : IRequest<Guid?>;

public sealed class GetTableByCodeHandler : IRequestHandler<GetTableByCodeQuery, Guid?>
{
    private readonly IApplicationDbContext _db;
    public GetTableByCodeHandler(IApplicationDbContext db) => _db = db;

    public async Task<Guid?> Handle(GetTableByCodeQuery q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q.Code)) return null;
        var code = q.Code.Trim().ToUpperInvariant();

        var table = await _db.Tables
            .Where(t => t.Code.ToUpper() == code)
            .Select(t => (Guid?)t.Id)
            .FirstOrDefaultAsync(ct);

        return table;
    }
}