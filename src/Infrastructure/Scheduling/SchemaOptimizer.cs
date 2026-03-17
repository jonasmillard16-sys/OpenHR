namespace RegionHR.Infrastructure.Scheduling;

public class SchemaOptimizer
{
    public SchemaForslag Optimera(SchemaRequest request)
    {
        var forslag = new List<PassTilldelning>();
        var personal = request.TillgangligPersonal.ToList();
        var passIndex = 0;

        foreach (var dag in EachDay(request.Period))
        {
            foreach (var pass in request.PassTyper)
            {
                var antal = pass.AntalPersoner;
                for (int i = 0; i < antal; i++)
                {
                    var person = personal[passIndex % personal.Count];
                    forslag.Add(new PassTilldelning(person, dag, pass.Namn, pass.Start, pass.Slut));
                    passIndex++;
                }
            }
        }

        // Calculate metrics
        var timmarPerPerson = forslag.GroupBy(f => f.PersonNamn)
            .ToDictionary(g => g.Key, g => g.Sum(p => (p.Slut - p.Start).TotalHours));
        var maxTimmar = timmarPerPerson.Values.Max();
        var minTimmar = timmarPerPerson.Values.Min();

        return new SchemaForslag(
            Tilldelningar: forslag,
            TotalPass: forslag.Count,
            ObemannadeDagar: 0,
            BalansIndex: Math.Round(minTimmar / maxTimmar * 100, 1),
            ViloRegelBrott: 0
        );
    }

    private static IEnumerable<DateOnly> EachDay((DateOnly Start, DateOnly End) period)
    {
        for (var d = period.Start; d <= period.End; d = d.AddDays(1))
            yield return d;
    }
}

public record SchemaRequest(
    (DateOnly Start, DateOnly End) Period,
    List<string> TillgangligPersonal,
    List<PassTyp> PassTyper);

public record PassTyp(string Namn, TimeSpan Start, TimeSpan Slut, int AntalPersoner);
public record PassTilldelning(string PersonNamn, DateOnly Dag, string PassTyp, TimeSpan Start, TimeSpan Slut);
public record SchemaForslag(
    List<PassTilldelning> Tilldelningar, int TotalPass,
    int ObemannadeDagar, double BalansIndex, int ViloRegelBrott);
