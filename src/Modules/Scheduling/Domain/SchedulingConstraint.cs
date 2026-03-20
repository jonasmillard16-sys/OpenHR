namespace RegionHR.Scheduling.Domain;

/// <summary>
/// Schemaläggningsbegränsning som används av optimeringslösaren.
/// Kan vara hård (ATL-lagkrav) eller mjuk (preferens/kostnad).
/// Typer: ATL, Avtal, Kompetens, Preferens, Kostnad.
/// </summary>
public sealed class SchedulingConstraint
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>ATL / Avtal / Kompetens / Preferens / Kostnad</summary>
    public string Typ { get; set; } = string.Empty;

    public string Beskrivning { get; set; } = string.Empty;

    /// <summary>Vikt vid optimering (högre = viktigare).</summary>
    public decimal Vikt { get; set; }

    /// <summary>True = hård begränsning (får inte brytas). False = mjuk (optimeras).</summary>
    public bool ArHard { get; set; }
}
