using Domain.Abstractions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class MenuItem : Entity<Guid>
{
    public int Number { get; private set; }

    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    public Money Price { get; private set; }

    public bool IsActive { get; private set; }

    private string? Description { get; set; }

    // New optional image fields
    public string? AvatarImageUrl { get; private set; }
    public string? BackgroundImageUrl { get; private set; }

    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }

    // For EF Core materialization
    private MenuItem() : base(default!) { }

    public MenuItem(Guid id, Guid categoryId, string name, string sku, Money price) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentNullException(nameof(sku));

        CategoryId = categoryId;
        Rename(name);
        SetSku(sku);
        ChangePrice(price);
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public void Rename(string name)
        => Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentNullException(nameof(name))
            : name.Trim();

    public void ChangePrice(Money price) => Price = price;

    public void SetAvatarImage(string? url)
        => AvatarImageUrl = NormalizeUrl(url);

    public void SetBackgroundImage(string? url)
        => BackgroundImageUrl = NormalizeUrl(url);

    private static string? NormalizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        return url.Trim();
    }

    // SKU is required for creation and immutable afterwards.
    private void SetSku(string sku)
    {
        Sku = string.IsNullOrWhiteSpace(sku)
            ? throw new ArgumentNullException(nameof(sku))
            : sku.Trim().ToUpperInvariant();
    }
}
