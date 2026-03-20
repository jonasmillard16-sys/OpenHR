using RegionHR.SharedKernel.Domain;

namespace RegionHR.Automation.Domain;

public sealed class AutomationSuggestion
{
    public Guid Id { get; private set; }
    public AutomationRuleId RegelId { get; private set; }
    public string ForeslagenAtgard { get; private set; } = string.Empty;
    public EmployeeId? SkapadFor { get; private set; }
    public SuggestionStatus Status { get; private set; }
    public DateTime SkapadVid { get; private set; }
    public DateTime GiltigTill { get; private set; }

    private AutomationSuggestion() { } // EF Core

    public static AutomationSuggestion Skapa(
        AutomationRuleId regelId,
        string foreslagenAtgard,
        EmployeeId? skapadFor = null,
        int giltigDagar = 7)
    {
        return new AutomationSuggestion
        {
            Id = Guid.NewGuid(),
            RegelId = regelId,
            ForeslagenAtgard = foreslagenAtgard,
            SkapadFor = skapadFor,
            Status = SuggestionStatus.Pending,
            SkapadVid = DateTime.UtcNow,
            GiltigTill = DateTime.UtcNow.AddDays(giltigDagar)
        };
    }

    public void Acceptera()
    {
        if (Status != SuggestionStatus.Pending)
            throw new InvalidOperationException("Kan bara acceptera väntande förslag");
        if (DateTime.UtcNow > GiltigTill)
            throw new InvalidOperationException("Förslaget har gått ut");

        Status = SuggestionStatus.Accepted;
    }

    public void Avvisa()
    {
        if (Status != SuggestionStatus.Pending)
            throw new InvalidOperationException("Kan bara avvisa väntande förslag");

        Status = SuggestionStatus.Dismissed;
    }
}
