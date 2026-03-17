using RegionHR.LAS.Domain;

namespace RegionHR.LAS.Services;

/// <summary>
/// Dashboard-vy för LAS-alarmeringar och status.
/// </summary>
public sealed class LASAlarmDashboard
{
    public int TotaltAktiva { get; init; }
    public int UnderGrans { get; init; }
    public int NaraGrans { get; init; }
    public int KritiskNara { get; init; }
    public int Konverterade { get; init; }
    public int MedForetradesratt { get; init; }
    public IReadOnlyList<LASAccumulation> TopNarmastKonvertering { get; init; } = [];
}
