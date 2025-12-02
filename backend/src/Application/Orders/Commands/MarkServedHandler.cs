using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;


namespace Application.Orders.Commands
{
    public sealed class MarkServedHandler : ICommandHandler<MarkServedCommand, OrderDto>
    {
        private readonly IApplicationDbContext _db;
        public MarkServedHandler(IApplicationDbContext db) => _db = db;
        public async Task<OrderDto> Handle(MarkServedCommand cmd, CancellationToken ct)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct)
                        ?? throw new KeyNotFoundException("Order not found");
            order.MarkServed();      // Ready -> Served :contentReference[oaicite:10]{index=10}
            await _db.SaveChangesAsync(ct);
            return OrderMapper.ToDto(order);
        }
    }
}
