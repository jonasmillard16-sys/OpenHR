namespace RegionHR.Positions.Domain;

public class HeadcountPlan
{
    public Guid Id { get; private set; }
    public Guid EnhetId { get; private set; }
    public int Ar { get; private set; }
    public int BudgeteradePositioner { get; private set; }
    public decimal BudgeteradFTE { get; private set; }
    public decimal BudgeteradKostnad { get; private set; }
    public int FaktiskaPositioner { get; set; }
    public decimal FaktiskFTE { get; set; }
    public decimal FaktiskKostnad { get; set; }
    public decimal Avvikelse => FaktiskKostnad - BudgeteradKostnad;
    public DateTime SkapadVid { get; private set; }

    private HeadcountPlan() { }

    public static HeadcountPlan Skapa(Guid enhetId, int ar, int budgeteradePositioner, decimal budgeteradFTE, decimal budgeteradKostnad)
    {
        return new HeadcountPlan
        {
            Id = Guid.NewGuid(), EnhetId = enhetId, Ar = ar,
            BudgeteradePositioner = budgeteradePositioner,
            BudgeteradFTE = budgeteradFTE, BudgeteradKostnad = budgeteradKostnad,
            SkapadVid = DateTime.UtcNow
        };
    }

    public void UppdateraFaktiskt(int positioner, decimal fte, decimal kostnad)
    {
        FaktiskaPositioner = positioner; FaktiskFTE = fte; FaktiskKostnad = kostnad;
    }
}
