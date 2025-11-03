using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Dòng hàng của Order. Có Id (int) để EF map gọn (OwnsMany).
/// </summary>
public class OrderItem
{
    public int Id { get; private set; }                      // EF key (int)
    public Guid MenuItemId { get; private set; }
    public string NameSnapshot { get; private set; } = default!;
    public Money UnitPrice { get; private set; }
    public Quantity Quantity { get; private set; }
    public string? Note { get; private set; }

    private OrderItem() { } // EF

    public OrderItem(Guid menuItemId, string nameSnapshot, Money unitPrice, Quantity quantity)
    {
        if (menuItemId == Guid.Empty) throw new ArgumentNullException(nameof(menuItemId));
        if (string.IsNullOrWhiteSpace(nameSnapshot)) throw new ArgumentNullException(nameof(nameSnapshot));
        MenuItemId = menuItemId;
        NameSnapshot = nameSnapshot.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }

    public Money LineTotal => new Money(UnitPrice.Amount * Quantity.Value, UnitPrice.Currency);

    public void ChangeQuantity(Quantity q)
    {
        if (q.Value <= 0) throw new ArgumentOutOfRangeException(nameof(q.Value));
        Quantity = q;
    }
    public void ChangeNote(string? note)
    {
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }
}

