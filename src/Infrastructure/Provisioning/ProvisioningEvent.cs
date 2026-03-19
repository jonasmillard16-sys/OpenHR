namespace RegionHR.Infrastructure.Provisioning;

public enum ProvisioningTrigger
{
    NyAnstallning,
    AvslutadAnstallning,
    EnhetsByte,
    Manuell
}

public enum ProvisioningAktion
{
    SkapaKonto,
    InaktiveraKonto,
    UppdateraGruppmedlemskap,
    TilldelaLicens,
    AterkallLicens,
    SkapaEpost,
    SparraPasserkort
}

public enum ProvisioningStatus
{
    /// <summary>Åtgärden har registrerats lokalt. Inget externt system har anropats.</summary>
    RegistreradLokalt,

    /// <summary>Framtida status: extern provider har utfört åtgärden.</summary>
    ExekveradExternt,

    /// <summary>Framtida status: extern provider rapporterade fel.</summary>
    MisslyckadExternt
}

/// <summary>
/// Historiklogg för provisioneringsåtgärder.
/// I v1 registreras alla events lokalt via LocalRecordingProvider.
/// Ingen extern provisionering utförs.
/// </summary>
public class ProvisioningEvent
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public string AnstallNamn { get; private set; } = default!;
    public string TargetSystem { get; private set; } = default!;
    public ProvisioningAktion Aktion { get; private set; }
    public ProvisioningTrigger Trigger { get; private set; }
    public ProvisioningStatus Status { get; private set; }
    public DateTime Tidpunkt { get; private set; }
    public string? Detaljer { get; private set; }

    private ProvisioningEvent() { }

    public static ProvisioningEvent Skapa(
        Guid anstallId,
        string anstallNamn,
        string targetSystem,
        ProvisioningAktion aktion,
        ProvisioningTrigger trigger,
        string? detaljer = null)
    {
        return new ProvisioningEvent
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            AnstallNamn = anstallNamn,
            TargetSystem = targetSystem,
            Aktion = aktion,
            Trigger = trigger,
            Status = ProvisioningStatus.RegistreradLokalt,
            Tidpunkt = DateTime.UtcNow,
            Detaljer = detaljer
        };
    }
}
