namespace RegionHR.Infrastructure.Provisioning;

/// <summary>
/// Konfigurationspost som definierar vilka provisioneringsåtgärder
/// som ska skapas vid en given trigger.
/// Ren konfiguration — ingen exekveringslogik.
/// </summary>
public class ProvisioningRule
{
    public Guid Id { get; private set; }
    public ProvisioningTrigger Trigger { get; private set; }
    public string TargetSystem { get; private set; } = default!;
    public ProvisioningAktion Aktion { get; private set; }
    public bool ArAktiv { get; private set; }
    public string? Beskrivning { get; private set; }

    private ProvisioningRule() { }

    public static ProvisioningRule Skapa(
        ProvisioningTrigger trigger,
        string targetSystem,
        ProvisioningAktion aktion,
        string? beskrivning = null,
        bool aktiv = true)
    {
        return new ProvisioningRule
        {
            Id = Guid.NewGuid(),
            Trigger = trigger,
            TargetSystem = targetSystem,
            Aktion = aktion,
            ArAktiv = aktiv,
            Beskrivning = beskrivning
        };
    }
}
