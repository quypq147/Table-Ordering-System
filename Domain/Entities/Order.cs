using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Abstractions;
using Domain.Enums;
using Domain.Events;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Order : AggregateRoot<string>
{
    private readonly List<OrderItem> _items = new();

    public string TableId { get; private set; } = default!;
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? SubmittedAtUtc { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }

    // EF-friendly
    private Order() { }

    private Order(string id, string tableId) : base(id)
    {
        if (string.IsNullOrWhiteSpace(tableId)) throw new ArgumentNullException(nameof(tableId));
        TableId = tableId.Trim();
        CreatedAtUtc = DateTime.UtcNow;
        Raise(new OrderPlaced(id));
    }

    public static Order Start(string id, string tableId) => new(id, tableId);

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public void AddItem(string menuItemId, string nameSnapshot, Money unitPrice, Quantity quantity)
    {
        EnsureDraft();
        var existing = _items.FirstOrDefault(i => i.MenuItemId == menuItemId && i.UnitPrice.Currency == unitPrice.Currency);
        if (existing is null)
        {
            _items.Add(new OrderItem(menuItemId, nameSnapshot, unitPrice, quantity));
        }
        else
        {
            existing.Increase(quantity);
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
        if (_items.Count == 0) throw new InvalidOperationException("Cannot submit empty order.");
        Status = OrderStatus.Submitted;
        SubmittedAtUtc = DateTime.UtcNow;
        Raise(new OrderSubmitted(Id));
    }

    public void MarkInProgress()
    {
        EnsureState(OrderStatus.Submitted);
        Status = OrderStatus.InProgress;
    }

    public void MarkReady()
    {
        EnsureState(OrderStatus.InProgress);
        Status = OrderStatus.Ready;
    }

    public void MarkServed()
    {
        EnsureState(OrderStatus.Ready);
        Status = OrderStatus.Served;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Paid) throw new InvalidOperationException("Cannot cancel a paid order.");
        Status = OrderStatus.Cancelled;
    }

    public void Pay(Money amount)
    {
        // Allow paying in Served state (or earlier depending on policy)
        if (Status is not OrderStatus.Served and not OrderStatus.Ready and not OrderStatus.Submitted)
            throw new InvalidOperationException("Order is not payable in current state.");

        var due = Total();
        if (amount.Amount < due.Amount || !amount.Currency.Equals(due.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Insufficient or mismatched payment.");

        Status = OrderStatus.Paid;
        PaidAtUtc = DateTime.UtcNow;
        Raise(new OrderPaid(Id, amount.Amount, amount.Currency));
    }

    public Money Total() => _items.Aggregate(new Money(0, _items.FirstOrDefault()?.UnitPrice.Currency ?? "VND"),
                                             (acc, i) => acc + i.LineTotal);

    private void EnsureDraft()
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be modified.");
    }

    private void EnsureState(OrderStatus expected)
    {
        if (Status != expected) throw new InvalidOperationException($"Order must be {expected}.");
    }
}
