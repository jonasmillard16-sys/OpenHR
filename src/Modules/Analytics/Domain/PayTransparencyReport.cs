namespace RegionHR.Analytics.Domain;

public class PayTransparencyReport
{
    public Guid Id { get; private set; }
    public int Ar { get; private set; }
    public string RapportPeriod { get; private set; } = "";
    public string Status { get; private set; } = "Draft"; // Draft, Calculated, Published
    public DateTime GenereradVid { get; private set; }
    public DateTime? PubliceradVid { get; private set; }
    public int TotalAnstallda { get; private set; }
    public decimal KonsGapProcent { get; private set; }
    public decimal MedianGapProcent { get; private set; }
    public string RapportData { get; private set; } = "{}"; // JSON — detailed breakdown

    private readonly List<PayGapAnalysis> _analyser = [];
    public IReadOnlyList<PayGapAnalysis> Analyser => _analyser.AsReadOnly();

    private PayTransparencyReport() { }

    public static PayTransparencyReport Skapa(int ar, string rapportPeriod)
    {
        return new PayTransparencyReport
        {
            Id = Guid.NewGuid(),
            Ar = ar,
            RapportPeriod = rapportPeriod,
            Status = "Draft",
            GenereradVid = DateTime.UtcNow
        };
    }

    public void Berakna(
        int totalAnstallda,
        decimal konsGapProcent,
        decimal medianGapProcent,
        string rapportData,
        List<PayGapAnalysis> analyser)
    {
        if (Status == "Published")
            throw new InvalidOperationException("Kan inte beräkna om en publicerad rapport.");

        TotalAnstallda = totalAnstallda;
        KonsGapProcent = konsGapProcent;
        MedianGapProcent = medianGapProcent;
        RapportData = rapportData;
        Status = "Calculated";

        _analyser.Clear();
        _analyser.AddRange(analyser);
    }

    public void Publicera()
    {
        if (Status != "Calculated")
            throw new InvalidOperationException("Rapporten måste vara beräknad innan publicering.");

        Status = "Published";
        PubliceradVid = DateTime.UtcNow;
    }
}
