namespace RegionHR.Competence.Domain;

public class MandatoryTraining
{
    public Guid Id { get; private set; }
    public string RollNamn { get; private set; } = default!;
    public string UtbildningNamn { get; private set; } = default!;
    public int GiltighetManader { get; private set; }
    public string? Beskrivning { get; private set; }

    private MandatoryTraining() { }

    public static MandatoryTraining Skapa(string roll, string utbildning, int giltighetManader, string? beskrivning = null)
    {
        return new MandatoryTraining
        {
            Id = Guid.NewGuid(),
            RollNamn = roll,
            UtbildningNamn = utbildning,
            GiltighetManader = giltighetManader,
            Beskrivning = beskrivning
        };
    }
}
