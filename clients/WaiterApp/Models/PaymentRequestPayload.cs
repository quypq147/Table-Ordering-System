using System;

namespace WaiterApp.Models;

public sealed class PaymentRequestPayload
{
    public Guid OrderId { get; set; }
    public string TableCode { get; set; } = string.Empty;
}