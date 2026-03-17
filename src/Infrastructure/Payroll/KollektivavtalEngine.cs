namespace RegionHR.Infrastructure.Payroll;

public class KollektivavtalEngine
{
    // AB (Allmanna bestammelser) for regions
    public OBTillagg BeraknaOB(DateTime start, DateTime slut)
    {
        var timmar = (slut - start).TotalHours;
        decimal ob = 0;
        string typ = "";

        // Simplified OB logic
        var hour = start.Hour;
        var isWeekend = start.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        var isStorhelg = IsStorhelg(start);

        if (isStorhelg) { ob = (decimal)timmar * 130; typ = "Storhelg"; }
        else if (isWeekend) { ob = (decimal)timmar * 55; typ = "Helg"; }
        else if (hour >= 22 || hour < 6) { ob = (decimal)timmar * 113; typ = "Natt"; }
        else if (hour >= 19) { ob = (decimal)timmar * 46; typ = "Kvall"; }
        else { typ = "Dag (inget OB)"; }

        return new OBTillagg(Math.Round(ob, 0), typ, (decimal)timmar);
    }

    public ViloRegler KontrolleraVila(DateTime slutForraPass, DateTime startNastaPass)
    {
        var vilotid = (startNastaPass - slutForraPass).TotalHours;
        var minVila = 11.0; // AB kraver 11h, sjukvard kan ga ner till 9h
        var ok = vilotid >= minVila;
        return new ViloRegler(ok, vilotid, minVila);
    }

    public decimal BeraknaOvertid(decimal timmar, bool kvalificerad)
    {
        // Enkel overtid: 180%, Kvalificerad (natt/helg): 240%
        return kvalificerad ? timmar * 2.40m : timmar * 1.80m;
    }

    public SemesterRatt BeraknaSemester(int alder, int anstallningsmanader)
    {
        var dagar = alder >= 50 ? 32 : (alder >= 40 ? 31 : 25);
        var sparade = 0; // Max 5 years saved
        return new SemesterRatt(dagar, sparade, 5);
    }

    private static bool IsStorhelg(DateTime d)
    {
        // Jul, Nyar, Pask, Midsommar (simplified)
        return (d.Month == 12 && d.Day >= 24 && d.Day <= 26) ||
               (d.Month == 1 && d.Day == 1) ||
               (d.Month == 6 && d.Day >= 19 && d.Day <= 21);
    }
}

public record OBTillagg(decimal Belopp, string Typ, decimal Timmar);
public record ViloRegler(bool Godkand, double VilotidTimmar, double MinVilotid);
public record SemesterRatt(int ArligaDagar, int SparadeDagar, int MaxSparAr);
