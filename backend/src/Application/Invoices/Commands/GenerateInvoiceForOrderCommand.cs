using Application.Abstractions;

namespace Application.Invoices.Commands;

public sealed record GenerateInvoiceForOrderCommand(Guid OrderId) : ICommand<bool>;
