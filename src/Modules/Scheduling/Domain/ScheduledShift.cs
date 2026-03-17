using RegionHR.SharedKernel.Domain;

namespace RegionHR.Scheduling.Domain;

/// <summary>
/// Schemalagt pass för en anställd. Inkluderar planerad och faktisk tid (instämpling).
/// Spårar avvikelser mellan planerade och faktiska tider.
/// </summary>
public sealed class ScheduledShift
{
    public Guid Id { get; set; }
    public ScheduleId SchemaId { get; set; }
    public EmployeeId AnstallId { get; set; }
    public DateOnly Datum { get; set; }
    public ShiftType PassTyp { get; set; }

    // Planerad tid
    public TimeOnly PlaneradStart { get; set; }
    public TimeOnly PlaneradSlut { get; set; }
    public TimeSpan Rast { get; set; }

    // Faktisk tid (från instämpling)
    public TimeOnly? FaktiskStart { get; set; }
    public TimeOnly? FaktiskSlut { get; set; }

    public ShiftStatus Status { get; set; }
    public OBCategory OBKategori { get; set; }

    // Avvikelsespårning
    public AvvikelseTyp? Avvikelse { get; set; }
    public string? AvvikelseBeskrivning { get; set; }
    public bool HarAvvikelse => Avvikelse.HasValue;

    /// <summary>Övertid i timmar. Sätts när faktisk tid överstiger planerad med > 15 minuter.</summary>
    public decimal? OvertidTimmar { get; set; }

    /// <summary>Planerade arbetstimmar exklusive rast.</summary>
    public decimal PlaneradeTimmar
    {
        get
        {
            var total = (PlaneradSlut.ToTimeSpan() - PlaneradStart.ToTimeSpan());
            if (total < TimeSpan.Zero) total += TimeSpan.FromHours(24); // Nattpass
            return (decimal)(total - Rast).TotalHours;
        }
    }

    /// <summary>Faktiska arbetstimmar om instämplade.</summary>
    public decimal? FaktiskaTimmar
    {
        get
        {
            if (FaktiskStart is null || FaktiskSlut is null) return null;
            var total = (FaktiskSlut.Value.ToTimeSpan() - FaktiskStart.Value.ToTimeSpan());
            if (total < TimeSpan.Zero) total += TimeSpan.FromHours(24);
            return (decimal)(total - Rast).TotalHours;
        }
    }

    /// <summary>Stämpla in.</summary>
    public void StamplaIn(TimeOnly tid)
    {
        FaktiskStart = tid;
        Status = ShiftStatus.Pagaende;
    }

    /// <summary>Stämpla ut och beräkna eventuell övertid.</summary>
    public void StamplaUt(TimeOnly tid)
    {
        FaktiskSlut = tid;
        Status = ShiftStatus.Avslutad;

        // Beräkna övertid: om faktisk tid överstiger planerad med > 15 minuter
        var faktiska = FaktiskaTimmar;
        if (faktiska.HasValue)
        {
            var differens = faktiska.Value - PlaneradeTimmar;
            if (differens > 0.25m) // > 15 minuter
            {
                OvertidTimmar = Math.Round(differens, 2);
            }
        }
    }

    /// <summary>
    /// Registrera en avvikelse på passet.
    /// </summary>
    public void RegistreraAvvikelse(AvvikelseTyp typ, string beskrivning)
    {
        if (string.IsNullOrWhiteSpace(beskrivning))
            throw new ArgumentException("Beskrivning krävs för avvikelse.", nameof(beskrivning));

        Avvikelse = typ;
        AvvikelseBeskrivning = beskrivning;
    }
}

public enum ShiftType
{
    Dag,        // Dagpass (~07:00-16:00)
    Kvall,      // Kvällspass (~15:00-22:00)
    Natt,       // Nattpass (~21:00-07:00)
    Jour,       // Jourpass
    Beredskap,  // Beredskapstjänst
    Delat       // Delat pass
}

public enum ShiftStatus
{
    Planerad,
    Pagaende,
    Avslutad,
    Avbokad,
    Bytt
}

public enum AvvikelseTyp
{
    SenAnkomst,
    TidigAvgang,
    SaknadUtstampling,
    Overtid,
    EjPlaneratPass
}
