namespace RegionHR.Analytics.Domain;

public class PayGapAnalysis
{
    public Guid Id { get; private set; }
    public Guid PayTransparencyReportId { get; private set; }
    public string Befattningskategori { get; private set; } = "";
    public int AntalKvinnor { get; private set; }
    public int AntalMan { get; private set; }
    public decimal MedelLonKvinnor { get; private set; }
    public decimal MedelLonMan { get; private set; }
    public decimal MedianLonKvinnor { get; private set; }
    public decimal MedianLonMan { get; private set; }
    public decimal OjusteratGapProcent { get; private set; }
    public decimal? JusteratGapProcent { get; private set; }
    public string ForklarandeFaktorer { get; private set; } = "{}"; // JSON

    private readonly List<PayGapCohort> _kohorter = [];
    public IReadOnlyList<PayGapCohort> Kohorter => _kohorter.AsReadOnly();

    private PayGapAnalysis() { }

    public static PayGapAnalysis Skapa(
        Guid payTransparencyReportId,
        string befattningskategori,
        int antalKvinnor,
        int antalMan,
        decimal medelLonKvinnor,
        decimal medelLonMan,
        decimal medianLonKvinnor,
        decimal medianLonMan,
        decimal ojusteratGapProcent,
        decimal? justeratGapProcent,
        string forklarandeFaktorer,
        List<PayGapCohort>? kohorter = null)
    {
        var analysis = new PayGapAnalysis
        {
            Id = Guid.NewGuid(),
            PayTransparencyReportId = payTransparencyReportId,
            Befattningskategori = befattningskategori,
            AntalKvinnor = antalKvinnor,
            AntalMan = antalMan,
            MedelLonKvinnor = medelLonKvinnor,
            MedelLonMan = medelLonMan,
            MedianLonKvinnor = medianLonKvinnor,
            MedianLonMan = medianLonMan,
            OjusteratGapProcent = ojusteratGapProcent,
            JusteratGapProcent = justeratGapProcent,
            ForklarandeFaktorer = forklarandeFaktorer
        };

        if (kohorter != null)
            analysis._kohorter.AddRange(kohorter);

        return analysis;
    }

    public void LaggTillKohort(PayGapCohort kohort)
    {
        _kohorter.Add(kohort);
    }

    /// <summary>
    /// EU Pay Transparency Directive: gap > 5% without justification triggers joint pay assessment.
    /// </summary>
    public bool Kraver5ProcentUtredning =>
        OjusteratGapProcent > 5m && (!JusteratGapProcent.HasValue || JusteratGapProcent.Value > 5m);
}
