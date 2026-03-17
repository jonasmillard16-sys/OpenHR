namespace RegionHR.SharedKernel.Abstractions;

/// <summary>
/// Basinterface för alla integrationsadaptrar (Adapter Pattern).
/// </summary>
public interface IIntegrationAdapter
{
    string SystemName { get; }
    Task<IntegrationResult> ExecuteAsync(IntegrationRequest request, CancellationToken ct = default);
    Task<bool> HealthCheckAsync(CancellationToken ct = default);
}

public record IntegrationRequest(
    string OperationType,
    object Payload,
    Dictionary<string, string>? Metadata = null);

public record IntegrationResult(
    bool Success,
    string? Message = null,
    object? ResponseData = null,
    string? ErrorCode = null);
