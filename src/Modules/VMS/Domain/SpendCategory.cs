namespace RegionHR.VMS.Domain;

/// <summary>
/// Kostnadskategori för spend-analys.
/// </summary>
public sealed class SpendCategory
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = string.Empty;
    public string Beskrivning { get; private set; } = string.Empty;

    private SpendCategory() { } // EF Core

    public static SpendCategory Skapa(string namn, string beskrivning)
    {
        return new SpendCategory
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            Beskrivning = beskrivning
        };
    }
}
