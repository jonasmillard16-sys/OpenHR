namespace RegionHR.Positions.Domain;

public enum SuccessionReadiness { RedoNu, RedoInom1Ar, RedoInom2Ar, UnderUtveckling, EjIdentifierad }

public sealed class SuccessionPlan
{
    public Guid Id { get; private set; }
    public Guid PositionId { get; private set; }
    public Guid? NuvarandeInnehavare { get; private set; }
    public int? BeraknadPensionAr { get; private set; }
    public Guid? EftertradarKandidat { get; private set; }
    public SuccessionReadiness Beredskap { get; private set; }
    public int BeredskapProcent { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private SuccessionPlan() { }

    public static SuccessionPlan Skapa(Guid positionId, Guid? nuvarandeInnehavare, int? pensionAr, Guid? kandidat, SuccessionReadiness beredskap, int beredskapProcent)
    {
        return new SuccessionPlan
        {
            Id = Guid.NewGuid(), PositionId = positionId,
            NuvarandeInnehavare = nuvarandeInnehavare,
            BeraknadPensionAr = pensionAr,
            EftertradarKandidat = kandidat,
            Beredskap = beredskap,
            BeredskapProcent = Math.Clamp(beredskapProcent, 0, 100),
            SkapadVid = DateTime.UtcNow
        };
    }
}
