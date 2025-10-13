using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Commands
{
    public sealed class CancelOrderHandler : ICommandHandler<CancelOrderCommand, OrderDto>
    {
        private readonly IApplicationDbContext _db;
        public CancelOrderHandler(IApplicationDbContext db) => _db = db;
        public async Task<OrderDto> Handle(CancelOrderCommand cmd, CancellationToken ct)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct)
                        ?? throw new KeyNotFoundException("Order not found");
            order.Cancel();          // không cho hủy nếu Paid; set CancelledAtUtc :contentReference[oaicite:11]{index=11}
            await _db.SaveChangesAsync(ct);
            return OrderMapper.ToDto(order);
        }
    }
}
