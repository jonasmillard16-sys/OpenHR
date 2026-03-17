using System.Text;
using System.Xml;
using System.Xml.Linq;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.IntegrationHub.Adapters.Skatteverket;

/// <summary>
/// Genererar AGI-XML (Arbetsgivardeklaration på individnivå) per Skatteverkets spec.
/// Skapas månadsvis, innehåller bruttolön, skatteavdrag, förmåner och arbetsgivaravgifter per individ.
/// Max 1000 individer per fil.
/// </summary>
public sealed class AGIXmlGenerator
{
    private const string AGI_NAMESPACE = "http://xmls.skatteverket.se/se/skatteverket/ai/instans/inkomstdeklaration/1.1";
    private const int MAX_INDIVIDER_PER_FIL = 1000;

    /// <summary>
    /// Generera AGI-XML-filer för en löneperiod.
    /// Returnerar en eller flera XML-filer (max 1000 individer per fil).
    /// </summary>
    public IReadOnlyList<AGIFile> Generate(AGIInput input)
    {
        var files = new List<AGIFile>();
        var batches = input.Individer
            .Select((ind, i) => new { ind, batch = i / MAX_INDIVIDER_PER_FIL })
            .GroupBy(x => x.batch)
            .Select(g => g.Select(x => x.ind).ToList())
            .ToList();

        for (int i = 0; i < batches.Count; i++)
        {
            var xml = GenerateXml(input, batches[i], i + 1, batches.Count);
            var fileName = $"AGI_{input.Organisationsnummer}_{input.Period}_{i + 1:D3}.xml";
            files.Add(new AGIFile(fileName, xml));
        }

        return files;
    }

    private string GenerateXml(AGIInput input, List<AGIIndivid> individer, int filNr, int totaltAntalFiler)
    {
        var ns = XNamespace.Get(AGI_NAMESPACE);

        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "Arbetsgivardeklaration",
                new XAttribute("version", "1.1"),
                new XElement(ns + "Avsandare",
                    new XElement(ns + "Programnamn", "RegionHR"),
                    new XElement(ns + "Organisationsnummer", input.Organisationsnummer),
                    new XElement(ns + "TekniskKontaktperson",
                        new XElement(ns + "Namn", input.KontaktpersonNamn),
                        new XElement(ns + "Telefon", input.KontaktpersonTelefon),
                        new XElement(ns + "Epostadress", input.KontaktpersonEpost)
                    )
                ),
                new XElement(ns + "Blankettgemensamt",
                    new XElement(ns + "Arbetsgivare",
                        new XElement(ns + "AgRegistreradId", input.Organisationsnummer),
                        new XElement(ns + "Kontaktperson",
                            new XElement(ns + "Namn", input.KontaktpersonNamn),
                            new XElement(ns + "Telefon", input.KontaktpersonTelefon),
                            new XElement(ns + "Epostadress", input.KontaktpersonEpost)
                        )
                    ),
                    new XElement(ns + "Period", input.Period)
                ),
                new XElement(ns + "Blankett",
                    // Huvuduppgift (arbetsgivaravgifter)
                    new XElement(ns + "HU",
                        new XElement(ns + "AvgifterSumma",
                            FormatDecimal(individer.Sum(i => i.Arbetsgivaravgifter))),
                        new XElement(ns + "AvdragenSkattSumma",
                            FormatDecimal(individer.Sum(i => i.AvdragenSkatt))),
                        new XElement(ns + "SummaArbetsgivaravgifter",
                            FormatDecimal(individer.Sum(i => i.Arbetsgivaravgifter))),
                        new XElement(ns + "AvgiftsunderlagSumma",
                            FormatDecimal(individer.Sum(i => i.Avgiftsunderlag)))
                    ),
                    // Individuppgifter
                    individer.Select(ind => GenerateIndividElement(ns, ind))
                )
            )
        );

        using var writer = new StringWriter();
        using var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false
        });
        doc.WriteTo(xmlWriter);
        xmlWriter.Flush();
        return writer.ToString();
    }

    private static XElement GenerateIndividElement(XNamespace ns, AGIIndivid ind)
    {
        var element = new XElement(ns + "IU",
            new XElement(ns + "Personnummer", ind.Personnummer),
            new XElement(ns + "Namn", ind.Namn),
            // Fält 011: Kontant bruttolön
            new XElement(ns + "Falt011", FormatDecimal(ind.KontantBruttolonMm)),
            // Fält 012: Skatteavdrag
            new XElement(ns + "Falt012", FormatDecimal(ind.AvdragenSkatt))
        );

        // Fält 013: Förmåner (om > 0)
        if (ind.SkattepliktForman > 0)
            element.Add(new XElement(ns + "Falt013", FormatDecimal(ind.SkattepliktForman)));

        // Fält 018: Friskvårdsbidrag skattefritt (om > 0)
        if (ind.SkattefriForman > 0)
            element.Add(new XElement(ns + "Falt018", FormatDecimal(ind.SkattefriForman)));

        // Fält 050: Traktamente/resekostnader (om > 0)
        if (ind.Traktamente > 0)
            element.Add(new XElement(ns + "Falt050", FormatDecimal(ind.Traktamente)));

        // Fält 051: Milersättning (om > 0)
        if (ind.Milersattning > 0)
            element.Add(new XElement(ns + "Falt051", FormatDecimal(ind.Milersattning)));

        // Arbetsgivaravgifter
        element.Add(new XElement(ns + "Avgiftsunderlag", FormatDecimal(ind.Avgiftsunderlag)));
        element.Add(new XElement(ns + "Arbetsgivaravgifter", FormatDecimal(ind.Arbetsgivaravgifter)));

        // Anställningsperiod
        if (ind.AnstallningFrom.HasValue)
            element.Add(new XElement(ns + "AnstallningFrom", ind.AnstallningFrom.Value.ToString("yyyy-MM-dd")));
        if (ind.AnstallningTom.HasValue)
            element.Add(new XElement(ns + "AnstallningTom", ind.AnstallningTom.Value.ToString("yyyy-MM-dd")));

        return element;
    }

    private static string FormatDecimal(decimal value) => value.ToString("F0");
}

// Input/output models

public sealed class AGIInput
{
    public string Organisationsnummer { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;  // "YYYYMM"
    public string KontaktpersonNamn { get; set; } = string.Empty;
    public string KontaktpersonTelefon { get; set; } = string.Empty;
    public string KontaktpersonEpost { get; set; } = string.Empty;
    public List<AGIIndivid> Individer { get; set; } = [];
}

public sealed class AGIIndivid
{
    public string Personnummer { get; set; } = string.Empty;  // YYYYMMDDNNNN
    public string Namn { get; set; } = string.Empty;
    public decimal KontantBruttolonMm { get; set; }           // Fält 011
    public decimal AvdragenSkatt { get; set; }                 // Fält 012
    public decimal SkattepliktForman { get; set; }             // Fält 013
    public decimal SkattefriForman { get; set; }               // Fält 018
    public decimal Traktamente { get; set; }                   // Fält 050
    public decimal Milersattning { get; set; }                 // Fält 051
    public decimal Avgiftsunderlag { get; set; }
    public decimal Arbetsgivaravgifter { get; set; }
    public DateOnly? AnstallningFrom { get; set; }
    public DateOnly? AnstallningTom { get; set; }
}

public sealed record AGIFile(string FileName, string XmlContent);
