using Domain.Abstractions;
using Domain.Enums;
using Domain.Events;
using Domain.ValueObjects;
using Domain.Exceptions; // added

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

    // New: customer note (nullable)
    public string? CustomerNote { get; private set; }

    // Scalar
    public Guid TableId { get; private set; } = default!;
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

    // EF
    private Order() : base(default!) { }

    // ctor nhận code (chuẩn hoá và fallback)
    private Order(Guid id, Guid tableId, string code) : base(id)
    {
        if (tableId == Guid.Empty) throw new ArgumentNullException(nameof(tableId));
        TableId = tableId;
        Code = string.IsNullOrWhiteSpace(code) ? NewFallbackCode() : code.Trim();
    }

    // Giữ ctor cũ để tương thích
    private Order(Guid id, Guid tableId) : base(id)
    {
        if (tableId == Guid.Empty) throw new ArgumentNullException(nameof(tableId));
        TableId = tableId;
    }

    // factory chuẩn cho luồng Public
    public static Order Start(Guid id, Guid tableId, string code)
    {
        var order = new Order(id, tableId, code);
        order.Raise(new OrderPlaced(id));
        return order;
    }

    // Overload sinh fallback code
    public static Order Start(Guid id, Guid tableId)
    {
        var order = new Order(id, tableId, NewFallbackCode());
        order.Raise(new OrderPlaced(id));
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

        // xóa theo OrderItem.Id
        var removed = _items.RemoveAll(i => i.Id == orderItemId);

        // nếu không xóa được gì => client gửi sai Id, báo lỗi để frontend biết
        if (removed == 0)
        {
            throw new InvalidOperationException(
            $"Không tìm thấy dòng hàng với Id = {orderItemId} trong đơn.");
        }
    }

    // Xoá toàn bộ dòng hàng
    public void ClearItems()
    {
        EnsureDraft();
        _items.Clear();
    }

    /// <summary>
    /// Thay đổi ghi chú chung của đơn (chỉ khi đơn đang ở trạng thái Draft)
    /// </summary>
    public void ChangeCustomerNote(string? note)
    {
        EnsureDraft();
        CustomerNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
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
        Raise(new OrderInProgress(Id));
    }

    public void MarkReady()
    {
        EnsureState(OrderStatus.InProgress);
        OrderStatus = OrderStatus.Ready;
        ReadyAtUtc = DateTime.UtcNow;
        Raise(new OrderReady(Id));
    }

    public void MarkServed()
    {
        EnsureState(OrderStatus.Ready);
        OrderStatus = OrderStatus.Served;
        ServedAtUtc = DateTime.UtcNow;
        Raise(new OrderServed(Id));
    }

    public void Cancel()
    {
        if (OrderStatus == OrderStatus.Paid)
            throw new InvalidOperationException("Không thể hủy đơn đã thanh toán.");
        if (OrderStatus == OrderStatus.Cancelled) return;

        OrderStatus = OrderStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
        Raise(new OrderCancelled(Id));
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
            throw new ArgumentOutOfRangeException(nameof(newQuantity), "Số lượng phải lớn hơn hoặc bằng0");

        var line = _items.FirstOrDefault(i => i.Id == orderItemId)
        ?? throw new KeyNotFoundException($"Không tìm thấy OrderItem {orderItemId}");

        if (newQuantity == 0)
        {
            //0 => xóa dòng
            _items.Remove(line);
            return;
        }

        line.ChangeQuantity(new Quantity(newQuantity));
    }

    // đổi ghi chú cho dòng hàng
    public void ChangeItemNote(int orderItemId, string? note)
    {
        EnsureDraft();
        var line = _items.FirstOrDefault(i => i.Id == orderItemId)
        ?? throw new KeyNotFoundException($"Không tìm thấy OrderItem {orderItemId}");
        line.ChangeNote(note);
    }

    // Fallback code (OD-xxxx base36)
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

    // ===== Added payment helpers =====
    public void RequestCashPayment()
    {
        EnsureNotCancelled();
        if (OrderStatus == OrderStatus.Draft)
            throw DomainException.NotReadyForPayment();
        if (OrderStatus == OrderStatus.Paid)
            throw DomainException.AlreadyPaid();
        OrderStatus = OrderStatus.WaitingForPayment; // awaiting cashier collection
    }

    public void MarkPaidByTransfer()
    {
        EnsureNotCancelled();
        if (OrderStatus == OrderStatus.Paid)
            throw DomainException.AlreadyPaid();
        if (OrderStatus == OrderStatus.Draft)
            throw DomainException.NotReadyForPayment();
        OrderStatus = OrderStatus.Paid;
        PaidAtUtc = DateTime.UtcNow;
        var total = Total();
        Raise(new OrderPaid(Id, total.Amount, total.Currency, "Transfer"));
    }

    private void EnsureNotCancelled()
    {
        if (OrderStatus == OrderStatus.Cancelled)
            throw DomainException.Cancelled();
    }

    public Money CalculateSubTotal()
    {
        var sum = _items.Sum(i => i.UnitPrice.Amount * i.Quantity.Value);
        return new Money(sum, _items.FirstOrDefault()?.UnitPrice.Currency ?? "VND");
    }
}

