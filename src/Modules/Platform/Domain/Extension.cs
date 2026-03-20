namespace RegionHR.Platform.Domain;

public enum ExtensionTyp
{
    CustomObject,
    Workflow,
    Report,
    Integration
}

/// <summary>
/// Tillaggsdefinition i marknadsplatsen.
/// Representerar ett installerbart tillagg (paket).
/// </summary>
public sealed class Extension
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = string.Empty;
    public string Version { get; private set; } = string.Empty;
    public string Forfattare { get; private set; } = string.Empty;
    public string Beskrivning { get; private set; } = string.Empty;
    public ExtensionTyp Typ { get; private set; }
    public string Licens { get; private set; } = string.Empty;
    public string Kompatibilitet { get; private set; } = string.Empty;
    public string Innehall { get; private set; } = "{}"; // JSON
    public DateTime SkapadVid { get; private set; }

    private Extension() { } // EF Core

    public static Extension Skapa(
        string namn,
        string version,
        string forfattare,
        string beskrivning,
        ExtensionTyp typ,
        string licens,
        string kompatibilitet,
        string innehall)
    {
        if (string.IsNullOrWhiteSpace(namn))
            throw new ArgumentException("Namn kravs", nameof(namn));
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("Version kravs", nameof(version));
        if (string.IsNullOrWhiteSpace(forfattare))
            throw new ArgumentException("Forfattare kravs", nameof(forfattare));

        return new Extension
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            Version = version,
            Forfattare = forfattare,
            Beskrivning = beskrivning,
            Typ = typ,
            Licens = licens,
            Kompatibilitet = kompatibilitet,
            Innehall = innehall,
            SkapadVid = DateTime.UtcNow
        };
    }
}
