using RegionHR.SharedKernel.Domain;

namespace RegionHR.Scheduling.Services;

/// <summary>
/// Avgör OB-kategori baserat på datum och tidpunkt.
/// Implementeras av Payroll-modulen som har kunskap om svenska helgdagar (storhelger).
/// Injiceras vid runtime i Scheduling-modulens tjänster.
/// </summary>
public interface IOBKategoriProvider
{
    /// <summary>
    /// Beräkna vilken OB-kategori som gäller för en given tidpunkt.
    /// Tar hänsyn till:
    /// - Vardagkväll (18:00-22:00)
    /// - Vardagnatt (22:00-06:00)
    /// - Helg (fredag 22:00 till måndag 06:00)
    /// - Storhelg (svenska helgdagar och deras aftnar)
    /// </summary>
    OBCategory BeraknaKategori(DateOnly datum, TimeOnly tid);
}
