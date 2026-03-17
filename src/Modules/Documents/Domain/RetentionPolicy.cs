namespace RegionHR.Documents.Domain;

/// <summary>
/// Beräknar gallringstider enligt svensk lagstiftning och regionens dokumenthanteringspolicy.
/// </summary>
public static class RetentionPolicy
{
    /// <summary>
    /// Beräknar gallringsdatum baserat på dokumentkategori och referensdatum.
    /// </summary>
    /// <param name="category">Dokumentkategori</param>
    /// <param name="referenceDate">Referensdatum (t.ex. uppladdningsdatum eller anställningens slutdatum)</param>
    /// <returns>Datum då dokumentet kan gallras</returns>
    public static DateTime CalculateRetention(DocumentCategory category, DateTime referenceDate)
    {
        return category switch
        {
            // Lönespecifikationer: 7 år enligt bokföringslagen (7 kap. 2§ BFL)
            DocumentCategory.Lonespecifikation => referenceDate.AddYears(7),

            // Läkarintyg: 2 år
            DocumentCategory.Lakarintyg => referenceDate.AddYears(2),

            // Legitimationer: 10 år (krav för vårdpersonal)
            DocumentCategory.Legitimation => referenceDate.AddYears(10),

            // Betyg: 2 år (för ej anställda kandidater)
            DocumentCategory.Betyg => referenceDate.AddYears(2),

            // Anställningsavtal: 7 år efter avslut
            DocumentCategory.Anstallningsavtal => referenceDate.AddYears(7),

            // Övriga dokument: 5 år som standard
            _ => referenceDate.AddYears(5)
        };
    }
}
