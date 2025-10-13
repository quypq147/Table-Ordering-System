using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Abstractions;
using Domain.Enums;
using Domain.Events;
using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Aggregate Root: Order
/// EF-friendly: backing field cho Items, ctor không tham số, setter private.
/// </summary>
public class Order : AggregateRoot<string>
{
    // Backing field cho EF
    private readonly List<OrderItem> _items = new();

    // Scalar
    public string TableId { get; private set; } = default!;
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;

    // Timeline (UTC)
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? SubmittedAtUtc { get; private set; }
    public DateTime? InProgressAtUtc { get; private set; }
    public DateTime? ReadyAtUtc { get; private set; }
    public DateTime? ServedAtUtc { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }

    // Navigation (EF sẽ dùng backing field)
    public IReadOnlyCollection<OrderItem> Items => _items;

    private Order() { } // EF

    private Order(string id, string tableId) : base(id)
    {
        if (string.IsNullOrWhiteSpace(tableId)) throw new ArgumentNullException(nameof(tableId));
        TableId = tableId.Trim();
    }

    public static Order Start(string id, string tableId)
    {
        var order = new Order(id, tableId);
        order.Raise(new OrderPlaced(id));
        return order;
    }

    public void AddItem(string menuItemId, string menuItemName, Money unitPrice, Quantity quantity)
    {
        EnsureDraft();
        if (string.IsNullOrWhiteSpace(menuItemId)) throw new ArgumentNullException(nameof(menuItemId));
        if (string.IsNullOrWhiteSpace(menuItemName)) throw new ArgumentNullException(nameof(menuItemName));

        var existing = _items.FirstOrDefault(i => i.MenuItemId == menuItemId &&
            i.UnitPrice.Currency.Equals(unitPrice.Currency, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            _items.Add(new OrderItem(menuItemId.Trim(), menuItemName.Trim(), unitPrice, quantity));
        }
        else
        {
            existing.ChangeQuantity(new Quantity(existing.Quantity.Value + quantity.Value));
        }
    }

    public void RemoveItem(string menuItemId)
    {
        EnsureDraft();
        _items.RemoveAll(i => i.MenuItemId == menuItemId);
    }

    public void Submit()
    {
        EnsureDraft();
        if (_items.Count == 0) throw new InvalidOperationException("Không thể xác nhận đơn nếu đơn không có món.");
        Status = OrderStatus.Submitted;
        SubmittedAtUtc = DateTime.UtcNow;
        Raise(new OrderSubmitted(Id));
    }

    public void MarkInProgress()
    {
        EnsureState(OrderStatus.Submitted);
        Status = OrderStatus.InProgress;
        InProgressAtUtc = DateTime.UtcNow;
    }

    public void MarkReady()
    {
        EnsureState(OrderStatus.InProgress);
        Status = OrderStatus.Ready;
        ReadyAtUtc = DateTime.UtcNow;
    }

    public void MarkServed()
    {
        EnsureState(OrderStatus.Ready);
        Status = OrderStatus.Served;
        ServedAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Paid)
            throw new InvalidOperationException("Không thể cancel đơn .");
        if (Status == OrderStatus.Cancelled) return;

        Status = OrderStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
    }

    public void Pay(Money amount, string method)
    {
        if (Status != OrderStatus.Submitted && Status != OrderStatus.Ready && Status != OrderStatus.Served)
            throw new InvalidOperationException("Đơn này chưa thể thanh toán.");

        var due = Total();
        if (!amount.Currency.Equals(due.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Tiền tệ không hợp lệ.");
        if (amount.Amount != due.Amount)
            throw new InvalidOperationException($"Số tiền phải bằng số: {due.Amount}");

        Status = OrderStatus.Paid;
        PaidAtUtc = DateTime.UtcNow;
        Raise(new OrderPaid(Id, amount.Amount, amount.Currency, method));
    }

    public Money Total()
    {
        var currency = _items.FirstOrDefault()?.UnitPrice.Currency ?? "VND";
        var totalAmt = _items.Sum(i => i.UnitPrice.Amount * i.Quantity.Value);
        return new Money(totalAmt, currency);
    }

    private void EnsureDraft()
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be modified.");
    }

    private void EnsureState(OrderStatus expected)
    {
        if (Status != expected) throw new InvalidOperationException($"Order must be {expected}.");
    }
    public void ChangeItemQuantity(int orderItemId, int newQuantity)
    {
        if (newQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(newQuantity), "Quantity must be >= 0");

        var line = _items.FirstOrDefault(i => i.Id == orderItemId)
            ?? throw new KeyNotFoundException($"OrderItem {orderItemId} not found");

        if (newQuantity == 0)
        {
            // quy ước: 0 => xóa dòng
            _items.Remove(line);
            return;
        }

        line.ChangeQuantity(new Quantity(newQuantity));   // xem #2 bên dưới
        // Nếu bạn có UpdatedAt/DomainEvent thì cập nhật ở đây
    }
}

