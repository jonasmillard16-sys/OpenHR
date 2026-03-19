namespace RegionHR.Infrastructure.Payroll;

public class TraktamentsCalculator
{
    // Skatteverkets regler 2026 (baserat på 2025 + justering)
    public TraktamentsBerakning BeraknaInrikes(DateTime avresa, DateTime hemkomst, bool hotell)
    {
        var timmar = (hemkomst - avresa).TotalHours;
        decimal dagtraktamente = 0;
        decimal nattillagg = 0;
        string beskrivning;

        if (timmar < 4) { beskrivning = "Resa under 4 timmar — inget traktamente"; }
        else if (timmar < 10) { dagtraktamente = 130m; beskrivning = "Halvdag (4-10 timmar)"; }
        else { dagtraktamente = 260m; beskrivning = "Heldag (>10 timmar)"; }

        if (!hotell && timmar >= 20) { nattillagg = 130m; }

        var antalDagar = Math.Max(1, (int)Math.Ceiling(timmar / 24));
        var totalDagtraktamente = dagtraktamente * antalDagar;
        var totalNatt = nattillagg * Math.Max(0, antalDagar - 1);

        return new(totalDagtraktamente, totalNatt, totalDagtraktamente + totalNatt, antalDagar, beskrivning);
    }

    public TraktamentsBerakning BeraknaUtrikes(string land, DateTime avresa, DateTime hemkomst)
    {
        var dagbelopp = GetUtrikesTraktamente(land);
        var timmar = (hemkomst - avresa).TotalHours;
        var antalDagar = Math.Max(1, (int)Math.Ceiling(timmar / 24));
        var total = dagbelopp * antalDagar;
        return new(total, 0, total, antalDagar, $"Utrikes ({land}): {dagbelopp} kr/dag");
    }

    private static decimal GetUtrikesTraktamente(string land) => land.ToLower() switch
    {
        "norge" => 588m, "danmark" => 632m, "finland" => 500m,
        "tyskland" => 546m, "frankrike" => 621m, "storbritannien" => 698m,
        "usa" => 655m, "spanien" => 481m, "italien" => 530m,
        _ => 500m
    };
}

public record TraktamentsBerakning(decimal Dagtraktamente, decimal Natttillagg, decimal Totalt, int AntalDagar, string Beskrivning);
