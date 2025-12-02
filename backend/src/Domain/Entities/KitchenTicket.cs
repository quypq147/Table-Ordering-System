using Domain.Abstractions;
using Domain.Enums;

namespace Domain.Entities;

public sealed class KitchenTicket : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public int OrderItemId { get; private set; }
    public string ItemName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public KitchenTicketStatus Status { get; private set; } = KitchenTicketStatus.New;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? ReadyAtUtc { get; private set; }
    public DateTime? ServedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public string? CancelReason { get; private set; }

    private KitchenTicket() : base(default!) { }

    public KitchenTicket(Guid id, Guid orderId, int orderItemId, string itemName, int quantity) : base(id)
    {
        OrderId = orderId;
        OrderItemId = orderItemId;
        ItemName = string.IsNullOrWhiteSpace(itemName) ? throw new ArgumentNullException(nameof(itemName)) : itemName.Trim();
        Quantity = quantity <= 0 ? throw new ArgumentOutOfRangeException(nameof(quantity)) : quantity;
    }

    public void Start()
    {
        if (Status != KitchenTicketStatus.New) throw new InvalidOperationException("Ticket không ? tr?ng thái New.");
        Status = KitchenTicketStatus.InProgress;
        StartedAtUtc = DateTime.UtcNow;
    }

    public void MarkReady()
    {
        if (Status != KitchenTicketStatus.InProgress) throw new InvalidOperationException("Ticket ph?i ? tr?ng thái InProgress.");
        Status = KitchenTicketStatus.Ready;
        ReadyAtUtc = DateTime.UtcNow;
    }

    public void MarkServed()
    {
        if (Status != KitchenTicketStatus.Ready) throw new InvalidOperationException("Ticket ph?i ? tr?ng thái Ready.");
        Status = KitchenTicketStatus.Served;
        ServedAtUtc = DateTime.UtcNow;
    }
    public void Cancel(string? reason)
    {
        // Không cho hủy nếu đã served hoặc đã cancel
        if (Status == KitchenTicketStatus.Served || Status == KitchenTicketStatus.Cancelled)
        {
            throw new InvalidOperationException("Không thể hủy ticket đã hoàn tất hoặc đã hủy.");
        }

        Status = KitchenTicketStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
        CancelReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
    }
}
