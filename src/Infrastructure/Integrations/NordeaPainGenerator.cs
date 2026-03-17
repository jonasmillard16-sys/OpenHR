using System.Xml.Linq;

namespace RegionHR.Infrastructure.Integrations;

public class NordeaPainGenerator
{
    public string GeneratePain001(Pain001Data data)
    {
        var ns = XNamespace.Get("urn:iso:std:iso:20022:tech:xsd:pain.001.001.03");
        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "Document",
                new XElement(ns + "CstmrCdtTrfInitn",
                    new XElement(ns + "GrpHdr",
                        new XElement(ns + "MsgId", $"OPENHR-{DateTime.Now:yyyyMMddHHmmss}"),
                        new XElement(ns + "CreDtTm", DateTime.Now.ToString("o")),
                        new XElement(ns + "NbOfTxs", data.Betalningar.Count),
                        new XElement(ns + "CtrlSum", data.Betalningar.Sum(b => b.Belopp))),
                    new XElement(ns + "PmtInf",
                        data.Betalningar.Select(b =>
                            new XElement(ns + "CdtTrfTxInf",
                                new XElement(ns + "Amt",
                                    new XElement(ns + "InstdAmt", new XAttribute("Ccy", "SEK"), b.Belopp)),
                                new XElement(ns + "CdtrAcct",
                                    new XElement(ns + "Id",
                                        new XElement(ns + "IBAN", b.IBAN)))))))));
        return doc.ToString();
    }
}

public record Pain001Data(string AvsandareNamn, string AvsandareKonto, List<BetalningData> Betalningar);
public record BetalningData(string Namn, string IBAN, decimal Belopp, string Referens);
