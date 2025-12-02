using Application.Abstractions;
using Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Application.Tables.Commands;

public sealed class UpdateTableHandler : ICommandHandler<UpdateTableCommand, TableDto>
{
    private readonly IApplicationDbContext _db;
    public UpdateTableHandler(IApplicationDbContext db) => _db = db;
    public async Task<TableDto> Handle(UpdateTableCommand c, CancellationToken ct)
    {
        var t = await _db.Tables.FirstOrDefaultAsync(x => x.Id == c.Id, ct)
        ?? throw new KeyNotFoundException("Table not found");
        t.Update(c.Code, c.Seats);
        await _db.SaveChangesAsync(ct);
        return new TableDto(t.Id, t.Code, t.Seats, t.Status);
    }
}