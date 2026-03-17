namespace RegionHR.SharedKernel.Domain;

/// <summary>
/// Penningvärde i svenska kronor (SEK) med ören-precision.
/// Använder decimal för att undvika avrundningsfel i löneberäkningar.
/// </summary>
public readonly record struct Money : IComparable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "SEK")
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money SEK(decimal amount) => new(amount, "SEK");
    public static Money Zero => new(0m, "SEK");

    /// <summary>Avrunda till hela ören (2 decimaler)</summary>
    public Money RoundToOren() => new(Math.Round(Amount, 2, MidpointRounding.ToEven), Currency);

    /// <summary>Avrunda till hela kronor</summary>
    public Money RoundToKronor() => new(Math.Round(Amount, 0, MidpointRounding.ToEven), Currency);

    public static Money operator +(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator *(Money m, decimal factor) => new(m.Amount * factor, m.Currency);
    public static Money operator *(decimal factor, Money m) => new(m.Amount * factor, m.Currency);
    public static Money operator /(Money m, decimal divisor) => new(m.Amount / divisor, m.Currency);

    public static bool operator >(Money a, Money b) { EnsureSameCurrency(a, b); return a.Amount > b.Amount; }
    public static bool operator <(Money a, Money b) { EnsureSameCurrency(a, b); return a.Amount < b.Amount; }
    public static bool operator >=(Money a, Money b) { EnsureSameCurrency(a, b); return a.Amount >= b.Amount; }
    public static bool operator <=(Money a, Money b) { EnsureSameCurrency(a, b); return a.Amount <= b.Amount; }

    public int CompareTo(Money other)
    {
        EnsureSameCurrency(this, other);
        return Amount.CompareTo(other.Amount);
    }

    public override string ToString() => $"{Amount:N2} {Currency}";

    private static void EnsureSameCurrency(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException($"Kan inte blanda valutor: {a.Currency} och {b.Currency}");
    }
}
