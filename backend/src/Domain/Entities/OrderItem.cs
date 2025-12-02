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

    // NEW: allow customer note per cart line
    public string? Note { get; private set; }

    private OrderItem() { } // EF

    public OrderItem(Guid menuItemId, string nameSnapshot, Money unitPrice, Quantity quantity, string? note = null)
    {
        if (menuItemId == Guid.Empty) throw new ArgumentException("MenuItemId is required.", nameof(menuItemId));
        if (string.IsNullOrWhiteSpace(nameSnapshot)) throw new ArgumentException("NameSnapshot is required.", nameof(nameSnapshot));

        MenuItemId = menuItemId;
        NameSnapshot = nameSnapshot.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
        ChangeNote(note);
    }

    public void ChangeQuantity(Quantity q) => Quantity = q;

    public void ChangeNote(string? note)
        => Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
}

