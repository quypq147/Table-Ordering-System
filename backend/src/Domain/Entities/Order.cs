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
public class Order : AggregateRoot<Guid>
{
    // Backing field cho EF
    private readonly List<OrderItem> _items = new();
    public int Number { get; private set; }               
    public string Code { get; private set; } = default!;

    // Scalar
    public Guid TableId { get; private set; } = default!;
    public TableStatus Status { get; private set; } = TableStatus.Available;
    public OrderStatus OrderStatus { get; private set; } = OrderStatus.Draft;

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

    // NEW: ctor nhận code (chuẩn hoá và fallback)
    private Order(Guid id, Guid tableId, string code) : base(id)
    {
        if (tableId == Guid.Empty) throw new ArgumentNullException(nameof(tableId));
        TableId = tableId;
        Code = string.IsNullOrWhiteSpace(code) ? NewFallbackCode() : code.Trim();
    }

    // Giữ ctor cũ để tương thích nếu nơi khác còn gọi (sẽ không set Code -> nên ưu tiên dùng overload Start có code)
    private Order(Guid id, Guid tableId) : base(id)
    {
        if (tableId == Guid.Empty) throw new ArgumentNullException(nameof(tableId));
        TableId = tableId;
    }

    // NEW: factory chuẩn cho luồng Public (nhận code đã sinh ở Application)
    public static Order Start(Guid id, Guid tableId, string code)
    {
        var order = new Order(id, tableId, code);
        order.Raise(new OrderPlaced(id));
        return order;
    }

    // Giữ overload cũ để không vỡ compile, đồng thời sinh fallback code
    public static Order Start(Guid id, Guid tableId)
    {
        var order = new Order(id, tableId, NewFallbackCode());
        order.Raise(new OrderPlaced(id)); // nếu event đang nhận string, tạm ToString(); tốt hơn là đổi event sang Guid
        return order;
    }

    public void AddItem(Guid menuItemId, string menuItemName, Money unitPrice, Quantity quantity)
    {
        EnsureDraft();
        if (menuItemId == Guid.Empty) throw new ArgumentNullException(nameof(menuItemId));
        if (string.IsNullOrWhiteSpace(menuItemName)) throw new ArgumentNullException(nameof(menuItemName));

        var existing = _items.FirstOrDefault(i => i.MenuItemId == menuItemId &&
            i.UnitPrice.Currency.Equals(unitPrice.Currency, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            _items.Add(new OrderItem(menuItemId, menuItemName.Trim(), unitPrice, quantity));
        }
        else
        {
            existing.ChangeQuantity(new Quantity(existing.Quantity.Value + quantity.Value));
        }
    }

    public void RemoveItem(int orderItemId)
    {
        EnsureDraft();
        _items.RemoveAll(i => i.Id == orderItemId);
    }

    public void Submit()
    {
        EnsureDraft();
        if (_items.Count == 0) throw new InvalidOperationException("Không thể xác nhận đơn trống.");
        OrderStatus = OrderStatus.Submitted;
        SubmittedAtUtc = DateTime.UtcNow;
        Raise(new OrderSubmitted(Id));
    }

    public void MarkInProgress()
    {
        EnsureState(OrderStatus.Submitted);
        OrderStatus = OrderStatus.InProgress;
        InProgressAtUtc = DateTime.UtcNow;
    }

    public void MarkReady()
    {
        EnsureState(OrderStatus.InProgress);
        OrderStatus = OrderStatus.Ready;
        ReadyAtUtc = DateTime.UtcNow;
    }

    public void MarkServed()
    {
        EnsureState(OrderStatus.Ready);
        OrderStatus = OrderStatus.Served;
        ServedAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (OrderStatus == OrderStatus.Paid)
            throw new InvalidOperationException("Không thể hủy đơn đã thanh toán.");
        if (OrderStatus == OrderStatus.Cancelled) return;

        OrderStatus = OrderStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
    }

    public void Pay(Money amount, string method)
    {
        if (OrderStatus != OrderStatus.Submitted && OrderStatus != OrderStatus.Ready && OrderStatus != OrderStatus.Served)
            throw new InvalidOperationException("Đơn này chưa thể thanh toán.");

        var due = Total();
        if (!amount.Currency.Equals(due.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Tiền tệ không hợp lệ.");
        if (amount.Amount != due.Amount)
            throw new InvalidOperationException($"Số tiền phải bằng số: {due.Amount}");

        OrderStatus = OrderStatus.Paid;
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
        if (OrderStatus != OrderStatus.Draft)
            throw new InvalidOperationException("Chỉ đơn ở trạng thái Nháp mới được phép chỉnh sửa.");
    }

    private void EnsureState(OrderStatus expected)
    {
        if (OrderStatus != expected) throw new InvalidOperationException($"Đơn hàng phải ở trạng thái {expected}.");
    }

    public void ChangeItemQuantity(int orderItemId, int newQuantity)
    {
        if (newQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(newQuantity), "Số lượng phải lớn hơn hoặc bằng 0");

        var line = _items.FirstOrDefault(i => i.Id == orderItemId)
            ?? throw new KeyNotFoundException($"Không tìm thấy OrderItem {orderItemId}");

        if (newQuantity == 0)
        {
            // quy ước: 0 => xóa dòng
            _items.Remove(line);
            return;
        }

        line.ChangeQuantity(new Quantity(newQuantity));   // xem #2 bên dưới
        // Nếu bạn có UpdatedAt/DomainEvent thì cập nhật ở đây
    }

    // NEW: Fallback code (OD-xxxx base36) để tương thích khi chỗ khác còn gọi Start(id, tableId)
    private static string NewFallbackCode()
    {
        return "OD-" + Base36(Random.Shared.Next(36 * 36 * 36 * 36), 4);
    }

    private static string Base36(int value, int pad)
    {
        const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        Span<char> buf = stackalloc char[8];
        var i = buf.Length;
        var v = value;
        do
        {
            buf[--i] = alphabet[v % 36];
            v /= 36;
        } while (v > 0);
        var s = new string(buf[i..]);
        return s.Length >= pad ? s : s.PadLeft(pad, '0');
    }
}

