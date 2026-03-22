namespace RegionHR.SharedKernel.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
    DateOnly TodaySweden { get; }
}
