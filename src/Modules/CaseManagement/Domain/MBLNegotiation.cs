namespace RegionHR.CaseManagement.Domain;

public enum MBLStatus { Kallad, Pagaende, Avslutad, Protokollfrd }
public enum MBLType { Information, Forhandling }

public sealed class MBLNegotiation
{
    public Guid Id { get; private set; }
    public string Arende { get; private set; } = default!;
    public MBLType Typ { get; private set; }
    public MBLStatus Status { get; private set; }
    public DateOnly Datum { get; private set; }
    public string? Fackombud { get; private set; }
    public string? Arbetsgivarombud { get; private set; }
    public string? Protokoll { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private MBLNegotiation() { }

    public static MBLNegotiation Skapa(string arende, MBLType typ, DateOnly datum, string? fackombud = null, string? arbetsgivarombud = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(arende);
        return new MBLNegotiation { Id = Guid.NewGuid(), Arende = arende, Typ = typ, Status = MBLStatus.Kallad, Datum = datum, Fackombud = fackombud, Arbetsgivarombud = arbetsgivarombud, SkapadVid = DateTime.UtcNow };
    }

    public void Paborja() { if (Status != MBLStatus.Kallad) throw new InvalidOperationException("Kan bara påbörja kallad."); Status = MBLStatus.Pagaende; }
    public void Avsluta() { if (Status != MBLStatus.Pagaende) throw new InvalidOperationException("Kan bara avsluta pågående."); Status = MBLStatus.Avslutad; }
    public void RegistreraProtokoll(string protokoll) { ArgumentException.ThrowIfNullOrWhiteSpace(protokoll); Protokoll = protokoll; Status = MBLStatus.Protokollfrd; }
}
