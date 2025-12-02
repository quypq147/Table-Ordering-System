namespace Domain.ValueObjects;

public readonly struct Money : IComparable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "VND")
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Currency = string.IsNullOrWhiteSpace(currency) ? "VND" : currency.Trim().ToUpperInvariant();
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    public static Money operator +(Money a, Money b)
        => EnsureSame(a, b, new Money(a.Amount + b.Amount, a.Currency));

    public static Money operator -(Money a, Money b)
        => EnsureSame(a, b, new Money(a.Amount - b.Amount, a.Currency));

    public static Money operator *(Money a, int times)
        => new Money(a.Amount * times, a.Currency);

    public static bool operator >(Money a, Money b) => a.Amount > EnsureSame(a, b).Amount;
    public static bool operator <(Money a, Money b) => a.Amount < EnsureSame(a, b).Amount;
    public static bool operator >=(Money a, Money b) => a.Amount >= EnsureSame(a, b).Amount;
    public static bool operator <=(Money a, Money b) => a.Amount <= EnsureSame(a, b).Amount;

    public int CompareTo(Money other)
    {
        if (!Currency.Equals(other.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot compare different currencies.");
        return Amount.CompareTo(other.Amount);
    }

    public override string ToString() => $"{Amount:0.##} {Currency}";

    private static Money EnsureSame(Money a, Money b, Money? value = null)
    {
        if (!a.Currency.Equals(b.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Currency mismatch.");
        return value ?? b;
    }
}
