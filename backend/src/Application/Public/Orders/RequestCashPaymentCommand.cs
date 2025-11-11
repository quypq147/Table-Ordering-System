using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.Public.Orders;

public sealed record RequestCashPaymentCommand(Guid OrderId) : ICommand<OrderDto>;

public sealed class RequestCashPaymentHandler : ICommandHandler<RequestCashPaymentCommand, OrderDto>
{
 private readonly IApplicationDbContext _db;
 public RequestCashPaymentHandler(IApplicationDbContext db) => _db = db;

 public async Task<OrderDto> Handle(RequestCashPaymentCommand c, CancellationToken ct)
 {
 var order = await _db.Orders.Include(o => o.Items)
 .FirstOrDefaultAsync(o => o.Id == c.OrderId, ct)
 ?? throw new KeyNotFoundException("Không tìm th?y ??n.");

 order.RequestCashPayment();
 await _db.SaveChangesAsync(ct);
 return OrderMapper.ToDto(order);
 }
}
