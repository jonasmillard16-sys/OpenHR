namespace RegionHR.SharedKernel.Domain;

/// <summary>
/// Datumintervall med temporal logik. Används för anställningsperioder,
/// giltighetsperioder, schemaperioder m.m.
/// </summary>
public sealed record DateRange
{
    public DateOnly Start { get; }
    public DateOnly? End { get; }

    public DateRange(DateOnly start, DateOnly? end = null)
    {
        if (end.HasValue && end.Value < start)
            throw new ArgumentException($"Slutdatum ({end.Value}) kan inte vara före startdatum ({start}).");
        Start = start;
        End = end;
    }

    public bool IsOpenEnded => !End.HasValue;
    public int? DurationInDays => End.HasValue ? End.Value.DayNumber - Start.DayNumber + 1 : null;

    public bool Contains(DateOnly date) => date >= Start && (!End.HasValue || date <= End.Value);

    public bool Overlaps(DateRange other)
    {
        var thisEnd = End ?? DateOnly.MaxValue;
        var otherEnd = other.End ?? DateOnly.MaxValue;
        return Start <= otherEnd && other.Start <= thisEnd;
    }

    public bool IsActiveOn(DateOnly date) => Contains(date);

    /// <summary>Antal kalenderdagar i perioden, null om öppen.</summary>
    public int? CalendarDays => DurationInDays;

    /// <summary>Antal arbetsdagar (mån-fre) i perioden.</summary>
    public int? WorkDays
    {
        get
        {
            if (!End.HasValue) return null;
            var count = 0;
            for (var d = Start; d <= End.Value; d = d.AddDays(1))
            {
                if (d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
                    count++;
            }
            return count;
        }
    }

    public static DateRange Infinite(DateOnly start) => new(start);
    public static DateRange SingleDay(DateOnly date) => new(date, date);

    public override string ToString() => End.HasValue ? $"{Start:yyyy-MM-dd} -- {End.Value:yyyy-MM-dd}" : $"{Start:yyyy-MM-dd} -->";
}
