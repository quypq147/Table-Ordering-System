using Domain.ValueObjects;

namespace Domain.Entities;

public class OrderItem
{
    public string MenuItemId { get; private set; } = default!;
    public string NameSnapshot { get; private set; } = default!;
    public Money UnitPrice { get; private set; }
    public Quantity Quantity { get; private set; }

    // EF-friendly
    private OrderItem() { }

    public OrderItem(string menuItemId, string nameSnapshot, Money unitPrice, Quantity quantity)
    {
        if (string.IsNullOrWhiteSpace(menuItemId)) throw new ArgumentNullException(nameof(menuItemId));
        if (string.IsNullOrWhiteSpace(nameSnapshot)) throw new ArgumentNullException(nameof(nameSnapshot));
        MenuItemId = menuItemId.Trim();
        NameSnapshot = nameSnapshot.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public Money LineTotal => UnitPrice * (int)Quantity;

    public void Increase(Quantity q) => Quantity = new Quantity(Quantity.Value + q.Value);
    public void ChangeQuantity(Quantity q) => Quantity = q;
}
