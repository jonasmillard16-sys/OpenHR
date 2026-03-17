namespace RegionHR.Recruitment.Domain;

public class OnboardingChecklist
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public Guid VakansId { get; private set; }
    public DateTime Startdatum { get; private set; }
    public List<OnboardingItem> Items { get; private set; } = new();
    public bool AllaKlara => Items.Count > 0 && Items.All(i => i.Klar);
    public DateTime SkapadVid { get; private set; }

    private OnboardingChecklist() { }

    public static OnboardingChecklist Skapa(Guid anstallId, Guid vakansId, DateOnly startdatum)
    {
        var checklist = new OnboardingChecklist
        {
            Id = Guid.NewGuid(), AnstallId = anstallId, VakansId = vakansId,
            Startdatum = startdatum.ToDateTime(TimeOnly.MinValue), SkapadVid = DateTime.UtcNow
        };
        // Standard items
        checklist.Items.Add(new OnboardingItem("IT-utrustning beställd"));
        checklist.Items.Add(new OnboardingItem("Behörigheter uppsatta"));
        checklist.Items.Add(new OnboardingItem("Arbetsplats förberedd"));
        checklist.Items.Add(new OnboardingItem("Obligatoriska utbildningar bokade (HLR, brandskydd)"));
        checklist.Items.Add(new OnboardingItem("Välkomstmöte planerat"));
        checklist.Items.Add(new OnboardingItem("Mentor/fadder tilldelad"));
        return checklist;
    }

    public void MarkeraKlar(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= Items.Count)
            throw new ArgumentOutOfRangeException(nameof(itemIndex));
        Items[itemIndex].MarkeraKlar();
    }
}

public class OnboardingItem
{
    public Guid Id { get; private set; }
    public string Beskrivning { get; private set; } = "";
    public bool Klar { get; private set; }
    public DateTime? KlarVid { get; private set; }

    private OnboardingItem() { }
    public OnboardingItem(string beskrivning) { Id = Guid.NewGuid(); Beskrivning = beskrivning; }
    public void MarkeraKlar() { Klar = true; KlarVid = DateTime.UtcNow; }
}
