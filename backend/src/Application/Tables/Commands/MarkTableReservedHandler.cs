using Application.Abstractions;
using Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Application.Tables.Commands
{
    public sealed class MarkTableReservedHandler : ICommandHandler<MarkTableReservedCommand, TableDto>
    {
        private readonly IApplicationDbContext _db;
        public MarkTableReservedHandler(IApplicationDbContext db) => _db = db;

        public async Task<TableDto> Handle(MarkTableReservedCommand c, CancellationToken ct)
        {
            var t = await _db.Tables.FirstOrDefaultAsync(x => x.Id == c.Id, ct)
                    ?? throw new KeyNotFoundException("Table not found");
            // Reserved trạng thái đã bỏ; dùng InUse nếu cần giữ bàn
            t.MarkInUse();
            await _db.SaveChangesAsync(ct);
            return new TableDto(t.Id, t.Code, t.Seats, t.Status);
        }
    }

}
