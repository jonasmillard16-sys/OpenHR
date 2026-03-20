namespace RegionHR.Scheduling.Domain;

/// <summary>
/// Händelse som påverkar bemanningsbehovet under en viss period.
/// T.ex. helgdagar, evenemang, influensasäsong, semesterperiod.
/// </summary>
public sealed class DemandEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Namn { get; set; } = string.Empty;

    /// <summary>Typ: Helgdag, Evenemang, Influensasasong, Semesterperiod</summary>
    public string Typ { get; set; } = string.Empty;

    /// <summary>Påverkansgrad: multiplikator för bemanningsbehov (1.0 = ingen påverkan, 1.5 = 50% ökning).</summary>
    public decimal PaverkanGrad { get; set; } = 1.0m;

    public DateOnly DatumFran { get; set; }
    public DateOnly DatumTill { get; set; }
}
