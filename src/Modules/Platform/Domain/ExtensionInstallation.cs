namespace RegionHR.Platform.Domain;

public enum InstallationStatus
{
    Active,
    Disabled
}

/// <summary>
/// Representerar en installerad instans av ett tillagg.
/// Sparar konfiguration och status for installationen.
/// </summary>
public sealed class ExtensionInstallation
{
    public Guid Id { get; private set; }
    public Guid ExtensionId { get; private set; }
    public string Version { get; private set; } = string.Empty;
    public DateTime InstallationsDatum { get; private set; }
    public InstallationStatus Status { get; private set; }
    public string Konfiguration { get; private set; } = "{}"; // JSON

    private ExtensionInstallation() { } // EF Core

    public static ExtensionInstallation Installera(Guid extensionId, string version, string? konfiguration = null)
    {
        if (extensionId == Guid.Empty)
            throw new ArgumentException("ExtensionId kravs", nameof(extensionId));
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("Version kravs", nameof(version));

        return new ExtensionInstallation
        {
            Id = Guid.NewGuid(),
            ExtensionId = extensionId,
            Version = version,
            InstallationsDatum = DateTime.UtcNow,
            Status = InstallationStatus.Active,
            Konfiguration = konfiguration ?? "{}"
        };
    }

    public void Inaktivera()
    {
        if (Status == InstallationStatus.Disabled)
            throw new InvalidOperationException("Installationen ar redan inaktiverad.");
        Status = InstallationStatus.Disabled;
    }

    public void Aktivera()
    {
        if (Status == InstallationStatus.Active)
            throw new InvalidOperationException("Installationen ar redan aktiv.");
        Status = InstallationStatus.Active;
    }
}
