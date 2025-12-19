using System;

namespace WaiterApp.Models;

public sealed class PaymentRequestPayload
{
    public Guid OrderId { get; set; }

    public Guid TableId { get; set; }
    public string TableCode { get; set; } = string.Empty;
}