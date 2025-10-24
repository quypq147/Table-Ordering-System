using Domain.Abstractions;

namespace Domain.Entities;

public class Category : Entity<string>
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; } = 0;

    private Category() { } // EF

    public Category(string id, string name, string? description = null, int sortOrder = 0) : base(id)
    {
        Rename(name);
        ChangeDescription(description);
        ChangeSortOrder(sortOrder);
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

