namespace Domain.ValueObjects;

public readonly struct Quantity
{
    public int Value { get; }

    public Quantity(int value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Quantity must be > 0.");
        Value = value;
    }

    public static implicit operator int(Quantity q) => q.Value;
}
