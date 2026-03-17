using RegionHR.SharedKernel.Domain;

namespace RegionHR.Scheduling.Domain;

/// <summary>
/// Stämplingshändelse (kom-och-gå).
/// Stöder webb-terminaler, PWA med offline-synk, och manuell registrering.
/// </summary>
public sealed class TimeClockEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public EmployeeId AnstallId { get; set; }
    public ClockEventType Typ { get; set; }
    public DateTime Tidpunkt { get; set; }
    public ClockSource Kalla { get; set; }
    public string? IPAdress { get; set; }
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }
    public bool ArOfflineStampling { get; set; }
    public DateTime? SynkadVid { get; set; }
    public Guid? KopplatPassId { get; set; }

    public static TimeClockEvent StamplaIn(EmployeeId anstallId, ClockSource kalla, string? ip = null)
    {
        return new TimeClockEvent
        {
            AnstallId = anstallId,
            Typ = ClockEventType.In,
            Tidpunkt = DateTime.UtcNow,
            Kalla = kalla,
            IPAdress = ip
        };
    }

    public static TimeClockEvent StamplaUt(EmployeeId anstallId, ClockSource kalla, string? ip = null)
    {
        return new TimeClockEvent
        {
            AnstallId = anstallId,
            Typ = ClockEventType.Ut,
            Tidpunkt = DateTime.UtcNow,
            Kalla = kalla,
            IPAdress = ip
        };
    }
}

public enum ClockEventType { In, Ut, Raststart, Rastslut }
public enum ClockSource { Webbterminal, PWA, Manuell, Integration }
