using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence.Converters;

public static class Converters
{
    public static readonly ValueConverter<Money, decimal> MoneyToDecimal =
        new ValueConverter<Money, decimal>(
            v => v.Amount,
            v => new Money(v, "VND"));

    public static readonly ValueConverter<Quantity, int> QuantityToInt =
        new ValueConverter<Quantity, int>(
            v => v.Value,
            v => new Quantity(v));

    public static readonly ValueConverter<DateOnly, DateTime> DateOnlyToDateTime =
        new ValueConverter<DateOnly, DateTime>(
            v => v.ToDateTime(TimeOnly.MinValue),
            v => DateOnly.FromDateTime(v));
}

