using Application.Abstractions;

namespace Application.Invoices.Queries;

public sealed record GetInvoiceByOrderIdQuery(Guid OrderId) : IQuery<InvoiceDto?>;

public sealed class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public DateTime IssuedAtUtc { get; set; }
    public decimal Total { get; set; }
}
