namespace RegionHR.PolicyManagement.Domain;

public enum PolicyStatus
{
    Utkast,
    Publicerad,
    Arkiverad
}

public enum PolicyCategory
{
    ITSakerhet,
    Arbetsmiljo,
    GDPR,
    Personalhandbok,
    Medicinsk,
    Juridisk,
    Ovrigt
}

/// <summary>
/// En organisationspolicy som kan kräva bekräftelse från anställda.
/// Livscykel: Utkast → Publicerad → Arkiverad.
/// </summary>
public sealed class Policy
{
    public Guid Id { get; private set; }
    public string Titel { get; private set; } = default!;
    public string? Sammanfattning { get; private set; }
    public string Innehall { get; private set; } = default!;
    public PolicyCategory Kategori { get; private set; }
    public int Version { get; private set; }
    public PolicyStatus Status { get; private set; }
    public bool KraverBekraftelse { get; private set; }
    public DateTime SkapadVid { get; private set; }
    public DateTime? PubliceradVid { get; private set; }
    public string SkapadAv { get; private set; } = default!;

    private Policy() { }

    public static Policy Skapa(
        string titel,
        string innehall,
        PolicyCategory kategori,
        bool kraverBekraftelse,
        string skapadAv,
        string? sammanfattning = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(titel);
        ArgumentException.ThrowIfNullOrWhiteSpace(innehall);
        ArgumentException.ThrowIfNullOrWhiteSpace(skapadAv);

        return new Policy
        {
            Id = Guid.NewGuid(),
            Titel = titel,
            Innehall = innehall,
            Kategori = kategori,
            Version = 1,
            Status = PolicyStatus.Utkast,
            KraverBekraftelse = kraverBekraftelse,
            SkapadVid = DateTime.UtcNow,
            SkapadAv = skapadAv,
            Sammanfattning = sammanfattning
        };
    }

    public void Publicera()
    {
        if (Status != PolicyStatus.Utkast)
            throw new InvalidOperationException($"Kan bara publicera från Utkast. Nuvarande: {Status}");
        Status = PolicyStatus.Publicerad;
        PubliceradVid = DateTime.UtcNow;
    }

    public void Arkivera()
    {
        if (Status != PolicyStatus.Publicerad)
            throw new InvalidOperationException($"Kan bara arkivera från Publicerad. Nuvarande: {Status}");
        Status = PolicyStatus.Arkiverad;
    }
}
