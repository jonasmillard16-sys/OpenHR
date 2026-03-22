namespace RegionHR.SharedKernel.Domain;

using RegionHR.SharedKernel.Abstractions;

public sealed class SystemClock : IClock
{
    private static readonly TimeZoneInfo _swedenTz =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

    public DateTime UtcNow => DateTime.UtcNow;

    public DateOnly TodaySweden =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _swedenTz));
}
