using RegionHR.SharedKernel.Domain;

namespace RegionHR.Payroll.Domain;

/// <summary>
/// Löneresultat per anställd och period.
/// Innehåller hela brutto-till-netto-kedjan.
/// </summary>
public sealed class PayrollResult
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public PayrollRunId KorningsId { get; private set; }
    public EmployeeId AnstallId { get; private set; }
    public EmploymentId AnstallningsId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }

    // Grunduppgifter
    public Money Manadslon { get; set; }
    public decimal Sysselsattningsgrad { get; set; }
    public CollectiveAgreementType Kollektivavtal { get; set; }

    // Beräknade belopp
    public Money Brutto { get; set; } = Money.Zero;
    public Money SkattepliktBrutto { get; set; } = Money.Zero;
    public Money Skatt { get; set; } = Money.Zero;
    public Money Netto { get; set; } = Money.Zero;

    // Arbetsgivaravgifter
    public Money Arbetsgivaravgifter { get; set; } = Money.Zero;
    public decimal ArbetsgivaravgiftSats { get; set; }

    // Semester
    public Money Semesterlon { get; set; } = Money.Zero;
    public Money Semestertillagg { get; set; } = Money.Zero;
    public int SemesterdagarIntjanade { get; set; }
    public int SemesterdagarUttagna { get; set; }

    // Pension
    public Money Pensionsgrundande { get; set; } = Money.Zero;
    public Money Pensionsavgift { get; set; } = Money.Zero;

    // OB-tillägg
    public Money OBTillagg { get; set; } = Money.Zero;

    // Övertid
    public Money Overtidstillagg { get; set; } = Money.Zero;

    // Jour och beredskap
    public Money JourTillagg { get; set; } = Money.Zero;
    public Money BeredskapsTillagg { get; set; } = Money.Zero;

    // Sjuklön
    public Money Sjuklon { get; set; } = Money.Zero;
    public Money Karensavdrag { get; set; } = Money.Zero;

    // Föräldralön
    public Money ForaldraloneUtfyllnad { get; set; } = Money.Zero;

    // Avdrag
    public Money Loneutmatning { get; set; } = Money.Zero;
    public Money Fackavgift { get; set; } = Money.Zero;
    public Money OvrigaAvdrag { get; set; } = Money.Zero;

    // Rader (detaljerat)
    private readonly List<PayrollResultLine> _rader = [];
    public IReadOnlyList<PayrollResultLine> Rader => _rader.AsReadOnly();

    public void LaggTillRad(PayrollResultLine rad) => _rader.Add(rad);

    public static PayrollResult Skapa(
        PayrollRunId korningsId, EmployeeId anstallId, EmploymentId anstallningsId,
        int year, int month, Money manadslon, decimal sysselsattningsgrad,
        CollectiveAgreementType kollektivavtal)
    {
        return new PayrollResult
        {
            KorningsId = korningsId,
            AnstallId = anstallId,
            AnstallningsId = anstallningsId,
            Year = year,
            Month = month,
            Manadslon = manadslon,
            Sysselsattningsgrad = sysselsattningsgrad,
            Kollektivavtal = kollektivavtal
        };
    }
}

public sealed class PayrollResultLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string LoneartKod { get; set; } = string.Empty;
    public string Benamning { get; set; } = string.Empty;
    public decimal Antal { get; set; }          // Timmar, dagar, stycken
    public Money Sats { get; set; }              // Per enhet
    public Money Belopp { get; set; }            // Antal * Sats
    public TaxCategory Skattekategori { get; set; }
    public bool ArSemestergrundande { get; set; }
    public bool ArPensionsgrundande { get; set; }
    public string? Kostnadsstalle { get; set; }
    public string? Projekt { get; set; }
    public string? AGIFaltkod { get; set; }      // Skatteverkets fältkod för AGI
    public bool ArAvdrag { get; set; }
}
