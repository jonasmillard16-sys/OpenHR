using RegionHR.SharedKernel.Domain;

namespace RegionHR.Competence.Domain;

/// <summary>
/// En karriärväg med steg (t.ex. Sjuksköterska -> Specialistsjuksköterska -> Vårdenhetschef).
/// </summary>
public class CareerPath
{
    public CareerPathId Id { get; private set; }
    public string Namn { get; private set; } = default!;
    public string Bransch { get; private set; } = default!;
    public string? Beskrivning { get; private set; }

    private readonly List<CareerPathStep> _steg = [];
    public IReadOnlyList<CareerPathStep> Steg => _steg.AsReadOnly();

    private CareerPath() { }

    public static CareerPath Skapa(string namn, string bransch, string? beskrivning = null)
    {
        return new CareerPath
        {
            Id = CareerPathId.New(),
            Namn = namn,
            Bransch = bransch,
            Beskrivning = beskrivning
        };
    }

    public CareerPathStep LaggTillSteg(string befattning, int ordning, int typiskTidManader,
        string? kravdaSkills = null, int kravdErfarenhetManader = 0)
    {
        var steg = CareerPathStep.Skapa(Id, ordning, befattning, typiskTidManader, kravdaSkills, kravdErfarenhetManader);
        _steg.Add(steg);
        return steg;
    }
}

/// <summary>
/// Ett steg i en karriärväg.
/// </summary>
public class CareerPathStep
{
    public Guid Id { get; private set; }
    public CareerPathId CareerPathId { get; private set; }
    public int Ordning { get; private set; }
    public string Befattning { get; private set; } = default!;
    public int TypiskTidManader { get; private set; }

    /// <summary>JSON med krävda skills och nivåer</summary>
    public string? KravdaSkills { get; private set; }

    public int KravdErfarenhetManader { get; private set; }

    private CareerPathStep() { }

    internal static CareerPathStep Skapa(CareerPathId careerPathId, int ordning, string befattning,
        int typiskTidManader, string? kravdaSkills, int kravdErfarenhetManader)
    {
        return new CareerPathStep
        {
            Id = Guid.NewGuid(),
            CareerPathId = careerPathId,
            Ordning = ordning,
            Befattning = befattning,
            TypiskTidManader = typiskTidManader,
            KravdaSkills = kravdaSkills,
            KravdErfarenhetManader = kravdErfarenhetManader
        };
    }
}
