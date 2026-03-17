using RegionHR.HalsoSAM.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.HalsoSAM.Services;

/// <summary>
/// Kommande uppföljning (dag 14/90/180/365) som ännu inte utförts.
/// </summary>
public sealed class UpcomingFollowUp
{
    public Guid CaseId { get; init; }
    public EmployeeId AnstallId { get; init; }
    public int DagNr { get; init; }
    public DateTime PlanerdDatum { get; init; }
}

/// <summary>
/// Huvudtjänst för rehabiliteringsärenden (HälsoSAM).
/// Orchestrerar skapande, tilldelning, planering och uppföljning.
/// </summary>
public sealed class RehabService
{
    private readonly IRehabRepository _repository;

    public RehabService(IRehabRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Skapa rehabiliteringsärende från SickLeaveMonitor-signal.</summary>
    public async Task<RehabCase> SkapaFranSignalAsync(
        EmployeeId anstallId, RehabTrigger trigger, CancellationToken ct)
    {
        var rehabCase = RehabCase.Skapa(anstallId, trigger);
        await _repository.AddAsync(rehabCase, ct);
        return rehabCase;
    }

    /// <summary>Tilldela ärendeägare (HR-person) till ett ärende.</summary>
    public async Task TilldelaArendeagareAsync(Guid caseId, EmployeeId hrPerson, CancellationToken ct)
    {
        var rehabCase = await _repository.GetByIdAsync(caseId, ct)
            ?? throw new InvalidOperationException($"Rehabiliteringsärende {caseId} hittades inte.");

        rehabCase.TilldelaArendeagare(hrPerson);
        await _repository.UpdateAsync(rehabCase, ct);
    }

    /// <summary>Sätt rehabiliteringsplan för ett ärende.</summary>
    public async Task SattRehabPlanAsync(Guid caseId, string plan, CancellationToken ct)
    {
        var rehabCase = await _repository.GetByIdAsync(caseId, ct)
            ?? throw new InvalidOperationException($"Rehabiliteringsärende {caseId} hittades inte.");

        rehabCase.SattRehabPlan(plan);
        await _repository.UpdateAsync(rehabCase, ct);
    }

    /// <summary>Lägg till anteckning i ett ärende.</summary>
    public async Task LaggTillAnteckningAsync(Guid caseId, string text, EmployeeId forfattare, CancellationToken ct)
    {
        var rehabCase = await _repository.GetByIdAsync(caseId, ct)
            ?? throw new InvalidOperationException($"Rehabiliteringsärende {caseId} hittades inte.");

        rehabCase.LaggTillAnteckning(text, forfattare);
        await _repository.UpdateAsync(rehabCase, ct);
    }

    /// <summary>Avsluta rehabiliteringsärende med slutsats. Sätter GDPR-gallringsdatum.</summary>
    public async Task AvslutaAsync(Guid caseId, string slutsats, CancellationToken ct)
    {
        var rehabCase = await _repository.GetByIdAsync(caseId, ct)
            ?? throw new InvalidOperationException($"Rehabiliteringsärende {caseId} hittades inte.");

        rehabCase.Avsluta(slutsats);
        await _repository.UpdateAsync(rehabCase, ct);
    }

    /// <summary>
    /// Hämta aktiva ärenden, filtrerade per enhet om angivet.
    /// Notera: utan enhetskoppling i RehabCase returneras alla aktiva ärenden.
    /// </summary>
    public async Task<IReadOnlyList<RehabCase>> HamtaAktivaArendenAsync(
        OrganizationId? enhetId, CancellationToken ct)
    {
        // Returnerar alla aktiva ärenden.
        // I produktion med enhetskoppling kan filtrering ske i repository.
        return await _repository.GetAktivaAsync(ct);
    }

    /// <summary>
    /// Hämta kommande uppföljningar (dag 14/90/180/365) inom angivet antal dagar framåt.
    /// </summary>
    public async Task<IReadOnlyList<UpcomingFollowUp>> HamtaKommandeUppfoljningarAsync(
        int dagarFramat, CancellationToken ct)
    {
        var aktiva = await _repository.GetAktivaAsync(ct);
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(dagarFramat);
        var result = new List<UpcomingFollowUp>();

        foreach (var arende in aktiva)
        {
            var utfordaDagar = arende.Uppfoljningar.Select(u => u.DagNr).ToHashSet();

            CheckFollowUp(arende, 14, arende.Uppfoljning14Dagar, utfordaDagar, now, cutoff, result);
            CheckFollowUp(arende, 90, arende.Uppfoljning90Dagar, utfordaDagar, now, cutoff, result);
            CheckFollowUp(arende, 180, arende.Uppfoljning180Dagar, utfordaDagar, now, cutoff, result);
            CheckFollowUp(arende, 365, arende.Uppfoljning365Dagar, utfordaDagar, now, cutoff, result);
        }

        return result.OrderBy(u => u.PlanerdDatum).ToList();
    }

    private static void CheckFollowUp(
        RehabCase arende, int dagNr, DateTime? planerdDatum,
        HashSet<int> utfordaDagar, DateTime now, DateTime cutoff,
        List<UpcomingFollowUp> result)
    {
        if (planerdDatum is not null
            && !utfordaDagar.Contains(dagNr)
            && planerdDatum.Value >= now
            && planerdDatum.Value <= cutoff)
        {
            result.Add(new UpcomingFollowUp
            {
                CaseId = arende.Id,
                AnstallId = arende.AnstallId,
                DagNr = dagNr,
                PlanerdDatum = planerdDatum.Value
            });
        }
    }
}
