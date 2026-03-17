using RegionHR.SharedKernel.Domain;
using RegionHR.Scheduling.Domain;
using RegionHR.Core.Contracts;

namespace RegionHR.Scheduling.Services;

/// <summary>
/// Ger en realtidsöversikt över bemanningsläget per enhet.
/// Jämför planerad och faktisk bemanning mot bemanningsmallarnas krav.
/// Används i driftledningens bemanningsvy.
/// </summary>
public sealed class StaffingOverviewService
{
    private readonly IScheduledShiftRepository _shiftRepository;
    private readonly IStaffingTemplateRepository _templateRepository;
    private readonly ICoreHRModule _coreHR;

    public StaffingOverviewService(
        IScheduledShiftRepository shiftRepository,
        IStaffingTemplateRepository templateRepository,
        ICoreHRModule coreHR)
    {
        _shiftRepository = shiftRepository;
        _templateRepository = templateRepository;
        _coreHR = coreHR;
    }

    /// <summary>
    /// Hämta bemanningsöversikt för en enhet och ett datum.
    /// </summary>
    public async Task<BemanningsOversikt> HamtaOversiktAsync(
        OrganizationId enhetId,
        DateOnly datum,
        CancellationToken ct = default)
    {
        var enhet = await _coreHR.GetOrganizationUnitAsync(enhetId, ct);
        var enhetNamn = enhet?.Namn ?? "Okänd enhet";

        var pass = await _shiftRepository.HamtaPassForEnhetAsync(enhetId, datum, ct);
        var mall = await _templateRepository.HamtaAktivMallAsync(enhetId, datum, ct);

        var passStatus = new List<PassBemanningStatus>();

        if (mall is not null)
        {
            // Gruppera behov per passtyp/tid
            var veckodag = datum.DayOfWeek;
            var relevantaRader = mall.Rader.Where(r => r.Veckodag == veckodag).ToList();

            foreach (var rad in relevantaRader)
            {
                var planeratAntal = pass.Count(p =>
                    p.PassTyp == rad.PassTyp &&
                    p.PlaneradStart == rad.Start &&
                    p.Status != ShiftStatus.Avbokad);

                var faktisktAntal = pass.Count(p =>
                    p.PassTyp == rad.PassTyp &&
                    p.PlaneradStart == rad.Start &&
                    (p.Status == ShiftStatus.Pagaende || p.Status == ShiftStatus.Avslutad));

                var status = BeraknaTrafikljus(faktisktAntal, planeratAntal, rad.MinAntal);

                passStatus.Add(new PassBemanningStatus
                {
                    PassTyp = rad.PassTyp,
                    Start = rad.Start,
                    Slut = rad.Slut,
                    Planerad = planeratAntal,
                    Faktisk = faktisktAntal,
                    MinKrav = rad.MinAntal,
                    Status = status
                });
            }
        }
        else
        {
            // Ingen mall: gruppera pass per typ
            var grupperadePass = pass
                .Where(p => p.Status != ShiftStatus.Avbokad)
                .GroupBy(p => new { p.PassTyp, p.PlaneradStart, p.PlaneradSlut });

            foreach (var grupp in grupperadePass)
            {
                var planerat = grupp.Count();
                var faktiskt = grupp.Count(p =>
                    p.Status == ShiftStatus.Pagaende || p.Status == ShiftStatus.Avslutad);

                passStatus.Add(new PassBemanningStatus
                {
                    PassTyp = grupp.Key.PassTyp,
                    Start = grupp.Key.PlaneradStart,
                    Slut = grupp.Key.PlaneradSlut,
                    Planerad = planerat,
                    Faktisk = faktiskt,
                    MinKrav = 0,
                    Status = faktiskt >= planerat ? TrafikljusStatus.Gron : TrafikljusStatus.Gul
                });
            }
        }

        var overgripandeStatus = BeraknaOvergripandeStatus(passStatus);

        return new BemanningsOversikt
        {
            EnhetId = enhetId,
            EnhetNamn = enhetNamn,
            Datum = datum,
            PassStatus = passStatus,
            OvergripandeStatus = overgripandeStatus
        };
    }

    /// <summary>
    /// Hämta bemanningsöversikt per underenhet för en förvaltning.
    /// </summary>
    public async Task<IReadOnlyList<BemanningsOversikt>> HamtaOversiktPerEnhetAsync(
        OrganizationId forvaltningId,
        DateOnly datum,
        CancellationToken ct = default)
    {
        var enheter = await _coreHR.GetEmployeesByUnitAsync(forvaltningId, datum, ct);

        // Hämta distinkta enhets-IDn. Här approximerar vi genom att använda förvaltningens enheter.
        // I produktionsmiljö skulle vi ha en dedikerad metod för att lista underenheter.
        var forvaltning = await _coreHR.GetOrganizationUnitAsync(forvaltningId, ct);
        if (forvaltning is null) return [];

        // Om vi bara har förvaltningens ID, returnera den
        var oversikt = await HamtaOversiktAsync(forvaltningId, datum, ct);
        return [oversikt];
    }

    private static TrafikljusStatus BeraknaTrafikljus(int faktisk, int planerad, int minKrav)
    {
        if (faktisk >= planerad && planerad > 0)
            return TrafikljusStatus.Gron;
        if (faktisk >= minKrav && minKrav > 0)
            return TrafikljusStatus.Gul;
        if (minKrav > 0 && faktisk < minKrav)
            return TrafikljusStatus.Rod;

        // Om inget minimikrav, basera på planerad
        return faktisk >= planerad ? TrafikljusStatus.Gron : TrafikljusStatus.Gul;
    }

    private static TrafikljusStatus BeraknaOvergripandeStatus(List<PassBemanningStatus> passStatus)
    {
        if (passStatus.Count == 0) return TrafikljusStatus.Gron;
        if (passStatus.Any(p => p.Status == TrafikljusStatus.Rod)) return TrafikljusStatus.Rod;
        if (passStatus.Any(p => p.Status == TrafikljusStatus.Gul)) return TrafikljusStatus.Gul;
        return TrafikljusStatus.Gron;
    }
}

/// <summary>Bemanningsöversikt för en enhet och ett datum.</summary>
public sealed class BemanningsOversikt
{
    public OrganizationId EnhetId { get; init; }
    public string EnhetNamn { get; init; } = string.Empty;
    public DateOnly Datum { get; init; }
    public List<PassBemanningStatus> PassStatus { get; init; } = [];
    public TrafikljusStatus OvergripandeStatus { get; init; }
}

/// <summary>Bemanningsstatus per passtyp.</summary>
public sealed class PassBemanningStatus
{
    public ShiftType PassTyp { get; init; }
    public TimeOnly Start { get; init; }
    public TimeOnly Slut { get; init; }
    public int Planerad { get; init; }
    public int Faktisk { get; init; }
    public int MinKrav { get; init; }
    public TrafikljusStatus Status { get; init; }
}

/// <summary>
/// Trafikljusstatus.
/// Grön: faktisk >= planerad.
/// Gul: faktisk >= minKrav men under planerad.
/// Röd: faktisk under minKrav.
/// </summary>
public enum TrafikljusStatus
{
    Gron,
    Gul,
    Rod
}

/// <summary>
/// Repository-interface för StaffingTemplate.
/// </summary>
public interface IStaffingTemplateRepository
{
    Task<StaffingTemplate?> HamtaAktivMallAsync(OrganizationId enhetId, DateOnly datum, CancellationToken ct = default);
    Task<IReadOnlyList<StaffingTemplate>> HamtaMallarForEnhetAsync(OrganizationId enhetId, CancellationToken ct = default);
}
