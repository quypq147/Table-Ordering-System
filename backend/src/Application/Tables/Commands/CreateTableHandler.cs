using Application.Abstractions;
using Application.Dtos;
using Application.Tables.Commands;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class CreateTableHandler : ICommandHandler<CreateTableCommand, TableDto>
{
    private readonly IApplicationDbContext _db;
    public CreateTableHandler(IApplicationDbContext db) => _db = db;

    public async Task<TableDto> Handle(CreateTableCommand c, CancellationToken ct)
    {
        if (await _db.Tables.AnyAsync(t => t.Id == c.Id || t.Code == c.Code, ct))
            throw new InvalidOperationException("Hiện tại đã có bàn này.");

        var t = new Table(c.Id, c.Code, c.Seats);
        _db.Tables.Add(t);
        await _db.SaveChangesAsync(ct);
        return new TableDto(t.Id, t.Code, t.Seats, t.Status);
    }
}
