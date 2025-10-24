using Domain.Abstractions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class MenuItem : Entity<string>
{
    public string Name { get; private set; } = default!;
    public Money Price { get; private set; }
    public bool IsActive { get; private set; } = true;

    public string CategoryId { get; private set; } = default!;
    public Category? Category { get; private set; }

    private MenuItem() { }

    public MenuItem(string id, string name, Money price) : base(id)
    {
        Rename(name);
        ChangePrice(price);
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public void Rename(string name)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name.Trim();
    }

    public void ChangePrice(Money price) => Price = price;
}
