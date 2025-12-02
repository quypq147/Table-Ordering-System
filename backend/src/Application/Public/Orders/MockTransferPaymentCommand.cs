using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;
using Domain.Exceptions;

namespace Application.Public.Orders;

public sealed record MockTransferPaymentCommand(Guid OrderId) : ICommand<OrderDto>;

public sealed class MockTransferPaymentHandler : ICommandHandler<MockTransferPaymentCommand, OrderDto>
{
    private readonly IApplicationDbContext _db;
    public MockTransferPaymentHandler(IApplicationDbContext db) => _db = db;

    public async Task<OrderDto> Handle(MockTransferPaymentCommand c, CancellationToken ct)
    {
        var order = await _db.Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == c.OrderId, ct)
            ?? throw DomainException.NotFound("Order", c.OrderId);

        order.MarkPaidByTransfer();
        await _db.SaveChangesAsync(ct);
        return OrderMapper.ToDto(order);
    }
}
