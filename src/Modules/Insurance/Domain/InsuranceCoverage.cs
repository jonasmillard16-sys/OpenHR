namespace RegionHR.Insurance.Domain;

public enum InsuranceType { TGL, AGS, TFA, AFA, PSA, Tjanstepension, Ovrigt }

public sealed class InsuranceCoverage
{
    public Guid Id { get; private set; }
    public InsuranceType Typ { get; private set; }
    public string Namn { get; private set; } = default!;
    public string? Beskrivning { get; private set; }
    public string Forsakringsgivare { get; private set; } = default!;
    public bool ArAktiv { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private InsuranceCoverage() { }

    public static InsuranceCoverage Skapa(InsuranceType typ, string namn, string forsakringsgivare, string? beskrivning = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(namn);
        return new InsuranceCoverage
        {
            Id = Guid.NewGuid(), Typ = typ, Namn = namn, Forsakringsgivare = forsakringsgivare,
            Beskrivning = beskrivning, ArAktiv = true, SkapadVid = DateTime.UtcNow
        };
    }
}
