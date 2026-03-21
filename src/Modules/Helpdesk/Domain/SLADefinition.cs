using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.Helpdesk.Domain;

/// <summary>
/// SLA-definition med tider för svar och lösning.
/// Eskalering sker automatiskt om SLA överskrids.
/// </summary>
public sealed class SLADefinition : AggregateRoot<Guid>
{
    public string Namn { get; private set; } = string.Empty;
    public int ForsvarstidMinuter { get; private set; }
    public int LostidMinuter { get; private set; }
    public int? EskaleringEfterMinuter { get; private set; }
    public bool ArAktiv { get; private set; }

    private SLADefinition() { }

    public static SLADefinition Skapa(string namn, int forsvarstidMinuter, int lostidMinuter,
        int? eskaleringEfterMinuter = null, bool arAktiv = true)
    {
        return new SLADefinition
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            ForsvarstidMinuter = forsvarstidMinuter,
            LostidMinuter = lostidMinuter,
            EskaleringEfterMinuter = eskaleringEfterMinuter,
            ArAktiv = arAktiv
        };
    }

    public void Uppdatera(string namn, int forsvarstidMinuter, int lostidMinuter,
        int? eskaleringEfterMinuter, bool arAktiv)
    {
        Namn = namn;
        ForsvarstidMinuter = forsvarstidMinuter;
        LostidMinuter = lostidMinuter;
        EskaleringEfterMinuter = eskaleringEfterMinuter;
        ArAktiv = arAktiv;
        UpdatedAt = DateTime.UtcNow;
    }
}
