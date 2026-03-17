using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;
using RegionHR.Recruitment.Domain;

namespace RegionHR.Recruitment.Services;

/// <summary>
/// Tjänst för rekryteringsprocessen — vakanser, ansökningar, bedömning och onboarding.
/// </summary>
public sealed class RecruitmentService
{
    private readonly IRepository<Vacancy, Guid> _vacancyRepository;

    public RecruitmentService(
        IRepository<Vacancy, Guid> vacancyRepository)
    {
        _vacancyRepository = vacancyRepository;
    }

    /// <summary>
    /// Skapar en ny vakans.
    /// </summary>
    public async Task<Vacancy> SkapaVakansAsync(
        OrganizationId enhetId, string titel, string beskrivning,
        EmploymentType anstallningsform, DateOnly sistadag, CancellationToken ct)
    {
        var vacancy = Vacancy.Skapa(enhetId, titel, beskrivning, anstallningsform, sistadag);
        await _vacancyRepository.AddAsync(vacancy, ct);
        return vacancy;
    }

    /// <summary>
    /// Publicerar vakansen — internt, externt och/eller på Platsbanken.
    /// </summary>
    public async Task PubliceraAsync(
        Guid vakansId, bool externt, bool platsbanken, CancellationToken ct)
    {
        var vacancy = await _vacancyRepository.GetByIdAsync(vakansId, ct)
            ?? throw new InvalidOperationException($"Vakans {vakansId} hittades inte");

        vacancy.Publicera(externt, platsbanken);
        await _vacancyRepository.UpdateAsync(vacancy, ct);
    }

    /// <summary>
    /// Tar emot en ansökan till en publicerad vakans.
    /// </summary>
    public async Task<Application> TaEmotAnsokanAsync(
        Guid vakansId, string namn, string epost, string? cvFilId, CancellationToken ct)
    {
        var vacancy = await _vacancyRepository.GetByIdAsync(vakansId, ct)
            ?? throw new InvalidOperationException($"Vakans {vakansId} hittades inte");

        var application = vacancy.TaEmotAnsokan(namn, epost, cvFilId);
        await _vacancyRepository.UpdateAsync(vacancy, ct);
        return application;
    }

    /// <summary>
    /// Hämtar alla ansökningar för en vakans.
    /// </summary>
    public async Task<IReadOnlyList<Application>> HamtaAnsokngarAsync(
        Guid vakansId, CancellationToken ct)
    {
        var vacancy = await _vacancyRepository.GetByIdAsync(vakansId, ct)
            ?? throw new InvalidOperationException($"Vakans {vakansId} hittades inte");

        return vacancy.Ansokngar;
    }

    /// <summary>
    /// Bedömer en ansökan med poäng och kommentar.
    /// </summary>
    public async Task BedomAsync(
        Guid vakansId, Guid ansokanId, int poang, string kommentar, CancellationToken ct)
    {
        var vacancy = await _vacancyRepository.GetByIdAsync(vakansId, ct)
            ?? throw new InvalidOperationException($"Vakans {vakansId} hittades inte");

        var ansokan = vacancy.Ansokngar.FirstOrDefault(a => a.Id == ansokanId)
            ?? throw new InvalidOperationException($"Ansökan {ansokanId} hittades inte");

        ansokan.Bedoma(poang, kommentar);
        await _vacancyRepository.UpdateAsync(vacancy, ct);
    }
}
