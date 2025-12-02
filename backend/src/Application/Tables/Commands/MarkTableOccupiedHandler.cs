using Application.Abstractions;
using Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Application.Tables.Commands
{
    public sealed class MarkTableOccupiedHandler : ICommandHandler<MarkTableOccupiedCommand, TableDto>
    {
        private readonly IApplicationDbContext _db;
        public MarkTableOccupiedHandler(IApplicationDbContext db) => _db = db;

        public async Task<TableDto> Handle(MarkTableOccupiedCommand c, CancellationToken ct)
        {
            var t = await _db.Tables.FirstOrDefaultAsync(x => x.Id == c.Id, ct)
                    ?? throw new KeyNotFoundException("Table not found");
            t.MarkInUse(); // :contentReference[oaicite:18]{index=18}
            await _db.SaveChangesAsync(ct);
            return new TableDto(t.Id, t.Code, t.Seats, t.Status);
        }
    }
}
