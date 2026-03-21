namespace RegionHR.Helpdesk.Domain;

/// <summary>
/// Kategori för serviceärenden med hierarkiskt stöd (self-referencing).
/// Styr routing till köer och standard-SLA.
/// </summary>
public sealed class ServiceCategory
{
    public Guid Id { get; set; }
    public string Namn { get; set; } = string.Empty;
    public string Beskrivning { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public Guid? DefaultKoId { get; set; }
    public ServiceRequestPriority? DefaultPrioritet { get; set; }
    public Guid? DefaultSLAId { get; set; }

    public static ServiceCategory Skapa(string namn, string beskrivning,
        Guid? parentId = null, Guid? defaultKoId = null,
        ServiceRequestPriority? defaultPrioritet = null, Guid? defaultSLAId = null)
    {
        return new ServiceCategory
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            Beskrivning = beskrivning,
            ParentId = parentId,
            DefaultKoId = defaultKoId,
            DefaultPrioritet = defaultPrioritet,
            DefaultSLAId = defaultSLAId
        };
    }
}
