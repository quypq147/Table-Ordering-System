// Application/Orders/Commands/PayOrderCommand.cs
using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Application.Tables.Commands;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Commands;

public sealed record PayOrderCommand(
    Guid OrderId,
    decimal Amount,
    string Currency,
    string Method = "CASH" // CASH or TRANSFER
) : ICommand<OrderDto>;

public sealed class PayOrderHandler : ICommandHandler<PayOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _uow;
    private readonly IApplicationDbContext _db;
    private readonly ISender _sender;

    public PayOrderHandler(IOrderRepository orders, IUnitOfWork uow, IApplicationDbContext db, ISender sender)
    {
        _orders = orders; _uow = uow; _db = db; _sender = sender;
    }

    public async Task<OrderDto> Handle(PayOrderCommand command, CancellationToken ct)
    {
        // Load order for payment (domain validation happens inside)
        var order = await _orders.GetByIdAsync(command.OrderId, ct)
                    ?? throw new InvalidOperationException("không tìm thấy đơn.");

        var method = string.IsNullOrWhiteSpace(command.Method) ? "CASH" : command.Method.Trim();

        // Execute payment based on method
        if (method.Equals("TRANSFER", StringComparison.OrdinalIgnoreCase))
        {
            // Mark paid by transfer without requiring explicit amount (domain computes total)
            order.MarkPaidByTransfer();
        }
        else
        {
            // Default: CASH, validate amount & currency against order total
            order.Pay(new Money(command.Amount, command.Currency), method);
        }

        _orders.Update(order);
        await _uow.SaveChangesAsync(ct);

        // Generate invoice record from the fully-loaded order (with items)
        var orderWithItems = await _db.Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, ct) ?? throw new InvalidOperationException("Không tìm thấy đơn (invoice).");

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
        var invoice = Invoice.CreateFromOrder(orderWithItems, invoiceNumber);
        // Map payment method to enum if available
        if (Enum.TryParse<PaymentMethod>(method, true, out var pm))
        {
            typeof(Invoice).GetProperty("PaymentMethod")!.SetValue(invoice, pm);
        }

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(ct);

        // Auto mark table available to end session
        await _sender.Send(new MarkTableAvailableCommand(order.TableId), ct);

        return OrderMapper.ToDto(order);
    }
}
