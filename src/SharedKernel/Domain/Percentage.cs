namespace RegionHR.SharedKernel.Domain;

/// <summary>
/// Procentvärde (0-100). Används för sysselsättningsgrad, skattesatser etc.
/// </summary>
public readonly record struct Percentage
{
    public decimal Value { get; }

    public Percentage(decimal value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(value), $"Procent måste vara 0-100, var {value}");
        Value = value;
    }

    public decimal AsFraction => Value / 100m;
    public Money Of(Money amount) => amount * AsFraction;

    public static Percentage FullTime => new(100);
    public static Percentage HalfTime => new(50);

    public static implicit operator decimal(Percentage p) => p.Value;
    public override string ToString() => $"{Value:F2}%";
}
