namespace RegionHR.VMS.Domain;

/// <summary>
/// Bedömning av om en inhyrd person ska klassificeras som anställd eller uppdragstagare.
/// Baseras på 5 faktorer från Skatteverkets kriterier:
/// 1. Kontroll (hur, när, var arbetet utförs)
/// 2. Verktyg/utrustning (vem tillhandahåller)
/// 3. Ekonomiskt beroende (andel av intäkt från en uppdragsgivare)
/// 4. Integration (ingår i organisationen vs. extern)
/// 5. Varaktighet (långvarigt vs. avgränsat uppdrag)
/// </summary>
public sealed class ContractorClassification
{
    public Guid Id { get; private set; }
    public Guid ContingentWorkerId { get; private set; }
    public string BedömningsResultat { get; private set; } = string.Empty; // Employee/Contractor/Unclear
    public string RiskNivå { get; private set; } = string.Empty; // Low/Medium/High
    public string Faktorer { get; private set; } = "{}"; // JSON: control, tools, economic_dependency, integration, duration
    public string BedömdAv { get; private set; } = string.Empty;
    public DateTime BedömdVid { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private ContractorClassification() { } // EF Core

    public static ContractorClassification Bedöm(
        Guid contingentWorkerId,
        string bedömningsResultat,
        string riskNivå,
        string faktorer,
        string bedömdAv)
    {
        if (bedömningsResultat is not ("Employee" or "Contractor" or "Unclear"))
            throw new ArgumentException("BedömningsResultat måste vara Employee, Contractor eller Unclear.");

        if (riskNivå is not ("Low" or "Medium" or "High"))
            throw new ArgumentException("RiskNivå måste vara Low, Medium eller High.");

        return new ContractorClassification
        {
            Id = Guid.NewGuid(),
            ContingentWorkerId = contingentWorkerId,
            BedömningsResultat = bedömningsResultat,
            RiskNivå = riskNivå,
            Faktorer = faktorer,
            BedömdAv = bedömdAv,
            BedömdVid = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Beräkna risknivå baserat på 5 Skatteverkets bedömningsfaktorer.
    /// Returnerar (klassificering, risknivå).
    /// Varje faktor har ett värde 0-2 (0=talar för uppdragstagare, 1=neutralt, 2=talar för anställd).
    /// </summary>
    public static (string Resultat, string RiskNivå) BeräknaRisk(
        int kontroll,       // 0-2: grad av uppdragsgivarens kontroll
        int verktyg,        // 0-2: vem tillhandahåller verktyg
        int ekonomisktBeroende, // 0-2: grad av ekonomiskt beroende
        int integration,    // 0-2: grad av integration i organisationen
        int varaktighet)    // 0-2: varaktighet av uppdraget
    {
        var totalPoäng = kontroll + verktyg + ekonomisktBeroende + integration + varaktighet;

        var resultat = totalPoäng switch
        {
            >= 8 => "Employee",
            >= 4 => "Unclear",
            _ => "Contractor"
        };

        var risk = totalPoäng switch
        {
            >= 8 => "High",    // Hög risk — trolig anställning
            >= 4 => "Medium",  // Oklar — behöver djupare utredning
            _ => "Low"         // Låg risk — tydlig uppdragstagare
        };

        return (resultat, risk);
    }
}
