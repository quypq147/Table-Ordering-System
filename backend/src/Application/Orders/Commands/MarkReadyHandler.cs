using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Commands
{
    public sealed class MarkReadyHandler : ICommandHandler<MarkReadyCommand, OrderDto>
    {
        private readonly IApplicationDbContext _db;
        public MarkReadyHandler(IApplicationDbContext db) => _db = db;
        public async Task<OrderDto> Handle(MarkReadyCommand cmd, CancellationToken ct)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct)
                        ?? throw new KeyNotFoundException("Order not found");
            order.MarkReady();       // InProgress -> Ready :contentReference[oaicite:9]{index=9}
            await _db.SaveChangesAsync(ct);
            return OrderMapper.ToDto(order);
        }
    }
}
