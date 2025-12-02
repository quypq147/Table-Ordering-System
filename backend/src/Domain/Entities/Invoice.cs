using Domain.Abstractions;
using Domain.Enums;


namespace Domain.Entities;

public sealed class Invoice : AggregateRoot<Guid>
{
    public Guid OrderId { get; private set; }
    public string InvoiceNumber { get; private set; } = default!;
    public DateTime IssuedAtUtc { get; private set; } = DateTime.UtcNow;

    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal ServiceCharge { get; private set; }
    public decimal Total { get; private set; }
    public string Currency { get; private set; } = "VND";

    public PaymentMethod PaymentMethod { get; private set; }

    public string? TableCode { get; private set; }
    public string? CustomerName { get; private set; }

    // EF-friendly ctor: supply temporary id in base
    private Invoice() : base(Guid.NewGuid()) { } // for EF

    private Invoice(Guid id) : base(id) { }

    public static Invoice CreateFromOrder(Order order, string invoiceNumber)
    {
        if (order is null) throw new ArgumentNullException(nameof(order));

        var subTotal = order.CalculateSubTotal().Amount;
        var discount = 0m; // TODO: apply voucher if any
        var tax = 0m;
        var service = 0m;
        var total = subTotal - discount + tax + service;

        var id = Guid.NewGuid();
        var invoice = new Invoice(id)
        {
            OrderId = order.Id,
            InvoiceNumber = invoiceNumber,
            IssuedAtUtc = DateTime.UtcNow,
            SubTotal = subTotal,
            DiscountAmount = discount,
            TaxAmount = tax,
            ServiceCharge = service,
            Total = total,
            Currency = order.CalculateSubTotal().Currency,
            TableCode = null,
            CustomerName = null
        };

        // PaymentMethod not present on Order aggregate; leave default or map externally
        invoice.PaymentMethod = default;

        return invoice;
    }
}
