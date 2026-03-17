using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;
using RegionHR.Travel.Domain;

namespace RegionHR.Travel.Services;

/// <summary>
/// Tjänst för hantering av resekrav, traktamenten, milersättning och utlägg.
/// </summary>
public sealed class TravelService
{
    private readonly IRepository<TravelClaim, Guid> _repository;

    public TravelService(IRepository<TravelClaim, Guid> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Skapar ett nytt resekrav.
    /// </summary>
    public async Task<TravelClaim> SkapaResekravAsync(
        EmployeeId anstallId, string beskrivning, DateOnly datum, CancellationToken ct)
    {
        var claim = TravelClaim.Skapa(anstallId, beskrivning, datum);
        await _repository.AddAsync(claim, ct);
        return claim;
    }

    /// <summary>
    /// Lägger till traktamente (hela och halva dagar, Skatteverkets satser).
    /// </summary>
    public async Task<TravelClaim> LaggTillTraktamenteAsync(
        Guid claimId, int helaDagar, int halvaDagar, CancellationToken ct)
    {
        var claim = await _repository.GetByIdAsync(claimId, ct)
            ?? throw new InvalidOperationException($"Resekrav {claimId} hittades inte");

        claim.SattTraktamente(helaDagar, halvaDagar);
        await _repository.UpdateAsync(claim, ct);
        return claim;
    }

    /// <summary>
    /// Lägger till milersättning.
    /// </summary>
    public async Task<TravelClaim> LaggTillMilersattningAsync(
        Guid claimId, decimal mil, CancellationToken ct)
    {
        var claim = await _repository.GetByIdAsync(claimId, ct)
            ?? throw new InvalidOperationException($"Resekrav {claimId} hittades inte");

        claim.SattMilersattning(mil);
        await _repository.UpdateAsync(claim, ct);
        return claim;
    }

    /// <summary>
    /// Lägger till utlägg med valfritt kvitto.
    /// </summary>
    public async Task<TravelClaim> LaggTillUtlaggAsync(
        Guid claimId, string beskrivning, Money belopp, string? kvittoId, CancellationToken ct)
    {
        var claim = await _repository.GetByIdAsync(claimId, ct)
            ?? throw new InvalidOperationException($"Resekrav {claimId} hittades inte");

        claim.LaggTillUtlagg(beskrivning, belopp, kvittoId);
        await _repository.UpdateAsync(claim, ct);
        return claim;
    }

    /// <summary>
    /// Skickar in resekravet för attestering.
    /// </summary>
    public async Task SkickaInAsync(Guid claimId, CancellationToken ct)
    {
        var claim = await _repository.GetByIdAsync(claimId, ct)
            ?? throw new InvalidOperationException($"Resekrav {claimId} hittades inte");

        claim.SkickaIn();
        await _repository.UpdateAsync(claim, ct);
    }

    /// <summary>
    /// Attesterar (godkänner) ett resekrav.
    /// </summary>
    public async Task AttesteraAsync(Guid claimId, string attestant, CancellationToken ct)
    {
        var claim = await _repository.GetByIdAsync(claimId, ct)
            ?? throw new InvalidOperationException($"Resekrav {claimId} hittades inte");

        claim.Attestera(attestant);
        await _repository.UpdateAsync(claim, ct);
    }

    /// <summary>
    /// Hämtar resekrav som väntar på attestering av angiven attestant.
    /// </summary>
    public async Task<IReadOnlyList<TravelClaim>> HamtaForAttestAsync(
        string attestant, CancellationToken ct)
    {
        var alla = await _repository.GetAllAsync(ct);
        return alla
            .Where(c => c.Status == TravelClaimStatus.Inskickad)
            .ToList()
            .AsReadOnly();
    }
}
