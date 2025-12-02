using Domain.Abstractions;

namespace Domain.Entities;

public class Category : Entity<Guid>
{
    public int Number { get; private set; }                // auto-increment (DB sequence)
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; } = 0;
    private string? IconUrl { get; set; } = default!;

    // EF
    private Category() : base(default!) { }

    public Category(Guid id, string code, string name, string? description, int sortOrder = 0) : base(id)
    {
        Code = string.IsNullOrWhiteSpace(code) ? throw new ArgumentNullException(nameof(code)) : code.Trim();
        Rename(name);
        ChangeDescription(description);
        ChangeSortOrder(sortOrder);
        IsActive = true;
    }

    public void Rename(string name)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name.Trim();
    }

    public void ChangeDescription(string? description)
        => Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    public void ChangeSortOrder(int sortOrder) => SortOrder = sortOrder;

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

