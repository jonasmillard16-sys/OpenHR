using System.Xml.Linq;

namespace RegionHR.Infrastructure.Integrations;

public class AGIXmlGenerator
{
    public string GenerateArbetsgivardeklaration(AGIData data)
    {
        var ns = XNamespace.Get("urn:skatteverket:se:taxationdata:agi:2.0");
        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "Arbetsgivardeklaration",
                new XElement(ns + "Avsandare",
                    new XElement(ns + "Organisationsnummer", data.OrgNr)),
                new XElement(ns + "Period", data.Period),
                new XElement(ns + "Redovisningsperiod", data.Period),
                new XElement(ns + "Individuppgift",
                    data.Anstallda.Select(a =>
                        new XElement(ns + "Uppgiftslamnare",
                            new XElement(ns + "Personnummer", a.Personnummer),
                            new XElement(ns + "KontantErsattning", a.Brutto),
                            new XElement(ns + "AvdragenSkatt", a.Skatt),
                            new XElement(ns + "Arbetsgivaravgift", a.Avgift))))));
        return doc.ToString();
    }
}

public record AGIData(string OrgNr, string Period, List<AGIAnstallData> Anstallda);
public record AGIAnstallData(string Personnummer, decimal Brutto, decimal Skatt, decimal Avgift);
