using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Core.Domain;

public sealed class OrganizationUnit : AggregateRoot<OrganizationId>
{
    public string Namn { get; private set; } = string.Empty;
    public OrganizationUnitType Typ { get; private set; }
    public OrganizationId? OverordnadEnhetId { get; private set; }
    public string Kostnadsstalle { get; private set; } = string.Empty;
    public string? CFARKod { get; private set; }
    public EmployeeId? ChefId { get; private set; }
    public CollectiveAgreementId? DefaultAvtalsId { get; private set; }
    public DateRange Giltighet { get; private set; } = null!;

    private readonly List<OrganizationUnit> _underenheter = [];
    public IReadOnlyList<OrganizationUnit> Underenheter => _underenheter.AsReadOnly();

    private OrganizationUnit() { }

    public static OrganizationUnit Skapa(
        string namn,
        OrganizationUnitType typ,
        string kostnadsstalle,
        DateOnly giltigFran,
        OrganizationId? overordnadEnhetId = null,
        string? cfarKod = null)
    {
        return new OrganizationUnit
        {
            Id = OrganizationId.New(),
            Namn = namn,
            Typ = typ,
            Kostnadsstalle = kostnadsstalle,
            OverordnadEnhetId = overordnadEnhetId,
            CFARKod = cfarKod,
            Giltighet = DateRange.Infinite(giltigFran)
        };
    }

    public void TilldelaChef(EmployeeId chefId)
    {
        ChefId = chefId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SattDefaultKollektivavtal(CollectiveAgreementId avtalsId)
    {
        DefaultAvtalsId = avtalsId;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool ArAktiv(DateOnly datum) => Giltighet.IsActiveOn(datum);
}
