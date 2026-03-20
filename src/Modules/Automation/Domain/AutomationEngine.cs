using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.Automation.Domain;

public interface IAutomationEngine
{
    /// <summary>Utvärdera regler baserat på en domänhändelse</summary>
    Task EvaluateAsync(IDomainEvent domainEvent, CancellationToken ct = default);

    /// <summary>Utvärdera cron-baserade regler för en given kategori</summary>
    Task EvaluateCronAsync(string cronCategory, CancellationToken ct = default);
}
