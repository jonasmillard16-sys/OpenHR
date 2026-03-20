using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Automation.Domain;

public sealed class AutomationLevelConfig : Entity<Guid>
{
    public AutomationCategoryId KategoriId { get; private set; }
    public AutomationLevel ValdNiva { get; private set; }

    private AutomationLevelConfig() { } // EF Core

    public static AutomationLevelConfig Skapa(AutomationCategoryId kategoriId, AutomationLevel valdNiva)
    {
        return new AutomationLevelConfig
        {
            Id = Guid.NewGuid(),
            KategoriId = kategoriId,
            ValdNiva = valdNiva
        };
    }

    public void AndraNiva(AutomationLevel nyNiva, AutomationLevel minimum)
    {
        if (nyNiva < minimum)
            throw new InvalidOperationException(
                $"Nivån {nyNiva} är under minimumnivån {minimum} för denna kategori");

        ValdNiva = nyNiva;
        UpdatedAt = DateTime.UtcNow;
    }
}
