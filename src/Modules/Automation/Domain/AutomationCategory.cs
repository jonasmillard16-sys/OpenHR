using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Automation.Domain;

public sealed class AutomationCategory : Entity<AutomationCategoryId>
{
    public string Namn { get; private set; } = string.Empty;
    public string Beskrivning { get; private set; } = string.Empty;
    public string Ikon { get; private set; } = string.Empty;

    private AutomationCategory() { } // EF Core

    public static AutomationCategory Skapa(string namn, string beskrivning, string ikon)
    {
        if (string.IsNullOrWhiteSpace(namn))
            throw new ArgumentException("Namn krävs", nameof(namn));

        return new AutomationCategory
        {
            Id = AutomationCategoryId.New(),
            Namn = namn,
            Beskrivning = beskrivning,
            Ikon = ikon
        };
    }
}
