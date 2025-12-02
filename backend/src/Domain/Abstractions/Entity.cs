namespace Domain.Abstractions;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; } = default!;

    protected Entity() { }
    protected Entity(TId id) => Id = id;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public static bool operator ==(Entity<TId>? a, Entity<TId>? b)
        => a is null && b is null || a is not null && a.Equals(b);

    public static bool operator !=(Entity<TId>? a, Entity<TId>? b) => !(a == b);

    public override int GetHashCode() => Id?.GetHashCode() ?? 0;
}
