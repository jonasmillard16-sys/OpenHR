using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Payroll.Domain;

public sealed class PayrollRun : AggregateRoot<PayrollRunId>
{
    public int Year { get; private set; }
    public int Month { get; private set; }
    public string Period => $"{Year}-{Month:D2}";
    public PayrollRunStatus Status { get; private set; }
    public DateTime? StartadVid { get; private set; }
    public DateTime? AvslutadVid { get; private set; }
    public string? StartadAv { get; private set; }
    public string? GodkandAv { get; private set; }
    public int AntalAnstallda { get; private set; }
    public Money TotalBrutto { get; private set; } = Money.Zero;
    public Money TotalNetto { get; private set; } = Money.Zero;
    public Money TotalSkatt { get; private set; } = Money.Zero;
    public Money TotalArbetsgivaravgifter { get; private set; } = Money.Zero;
    public bool ArRetroaktiv { get; private set; }
    public string? RetroaktivtForPeriod { get; private set; }

    private readonly List<PayrollResult> _resultat = [];
    public IReadOnlyList<PayrollResult> Resultat => _resultat.AsReadOnly();

    private readonly List<string> _berakningsFel = [];
    public IReadOnlyList<string> BerakningsFel => _berakningsFel.AsReadOnly();
    public bool HarFel => _berakningsFel.Count > 0;

    public void LaggTillFel(EmployeeId anstallId, string felmeddelande)
    {
        _berakningsFel.Add($"{anstallId}: {felmeddelande}");
    }

    private PayrollRun() { }

    public static PayrollRun Skapa(int year, int month, string startadAv, bool retroaktiv = false, string? retroPeriod = null)
    {
        return new PayrollRun
        {
            Id = PayrollRunId.New(),
            Year = year,
            Month = month,
            Status = PayrollRunStatus.Skapad,
            StartadAv = startadAv,
            ArRetroaktiv = retroaktiv,
            RetroaktivtForPeriod = retroPeriod
        };
    }

    public void Paborja()
    {
        if (Status != PayrollRunStatus.Skapad)
            throw new InvalidOperationException($"Kan inte påbörja lönekörning med status {Status}");
        Status = PayrollRunStatus.Paborjad;
        StartadVid = DateTime.UtcNow;
    }

    public void LaggTillResultat(PayrollResult resultat)
    {
        _resultat.Add(resultat);
        AntalAnstallda = _resultat.Count;
        TotalBrutto = Money.SEK(_resultat.Sum(r => r.Brutto.Amount));
        TotalNetto = Money.SEK(_resultat.Sum(r => r.Netto.Amount));
        TotalSkatt = Money.SEK(_resultat.Sum(r => r.Skatt.Amount));
        TotalArbetsgivaravgifter = Money.SEK(_resultat.Sum(r => r.Arbetsgivaravgifter.Amount));
    }

    public void MarkeraSomBeraknad()
    {
        Status = PayrollRunStatus.Beraknad;
        AvslutadVid = DateTime.UtcNow;
    }

    public void Godkann(string godkandAv)
    {
        if (Status != PayrollRunStatus.Beraknad && Status != PayrollRunStatus.Granskad)
            throw new InvalidOperationException("Kan inte godkänna en lönekörning som inte är beräknad/granskad");
        Status = PayrollRunStatus.Godkand;
        GodkandAv = godkandAv;
    }

    public void MarkeraSomUtbetald()
    {
        if (Status != PayrollRunStatus.Godkand)
            throw new InvalidOperationException("Kan inte markera som utbetald innan godkännande");
        Status = PayrollRunStatus.Utbetald;
        RaiseDomainEvent(new PayrollRunCompletedEvent(Id, Year, Month, TotalBrutto, TotalNetto));
    }
}

public sealed record PayrollRunCompletedEvent(
    PayrollRunId RunId, int Year, int Month,
    Money TotalBrutto, Money TotalNetto) : DomainEvent;
