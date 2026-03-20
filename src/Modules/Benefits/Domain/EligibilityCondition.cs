using System.Text.Json;

namespace RegionHR.Benefits.Domain;

public class EligibilityCondition
{
    public Guid Id { get; private set; }
    public Guid EligibilityRuleId { get; private set; }
    public string Falt { get; private set; } = "";
    public string Operator { get; private set; } = "";
    public string Varde { get; private set; } = "";

    private EligibilityCondition() { }

    private static readonly string[] GiltigaFalt =
    [
        "AnstallningsForm", "Sysselsattningsgrad", "Anstallningstid",
        "Alder", "Befattningskategori", "CollectiveAgreement", "OrganizationUnit"
    ];

    private static readonly string[] GiltigaOperatorer =
    [
        "IN", "NOT_IN", "GE", "LE", "EQ", "BETWEEN"
    ];

    public static EligibilityCondition Skapa(Guid eligibilityRuleId, string falt, string operatorTyp, string varde)
    {
        if (!GiltigaFalt.Contains(falt))
            throw new ArgumentException($"Ogiltigt fält: {falt}. Giltiga: {string.Join(", ", GiltigaFalt)}", nameof(falt));
        if (!GiltigaOperatorer.Contains(operatorTyp))
            throw new ArgumentException($"Ogiltig operator: {operatorTyp}. Giltiga: {string.Join(", ", GiltigaOperatorer)}", nameof(operatorTyp));

        return new EligibilityCondition
        {
            Id = Guid.NewGuid(),
            EligibilityRuleId = eligibilityRuleId,
            Falt = falt,
            Operator = operatorTyp,
            Varde = varde
        };
    }

    public bool Utvardera(Dictionary<string, string> anstallningsData)
    {
        if (!anstallningsData.TryGetValue(Falt, out var aktuellt))
            return false;

        return Operator switch
        {
            "EQ" => string.Equals(aktuellt, ParseSingleValue(), StringComparison.OrdinalIgnoreCase),
            "GE" => decimal.TryParse(aktuellt, out var ge) && decimal.TryParse(ParseSingleValue(), out var geV) && ge >= geV,
            "LE" => decimal.TryParse(aktuellt, out var le) && decimal.TryParse(ParseSingleValue(), out var leV) && le <= leV,
            "IN" => ParseArrayValue().Any(v => string.Equals(aktuellt, v, StringComparison.OrdinalIgnoreCase)),
            "NOT_IN" => !ParseArrayValue().Any(v => string.Equals(aktuellt, v, StringComparison.OrdinalIgnoreCase)),
            "BETWEEN" => EvalBetween(aktuellt),
            _ => false
        };
    }

    private string ParseSingleValue()
    {
        try
        {
            return JsonSerializer.Deserialize<string>(Varde) ?? Varde;
        }
        catch
        {
            return Varde;
        }
    }

    private string[] ParseArrayValue()
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(Varde) ?? [Varde];
        }
        catch
        {
            return [Varde];
        }
    }

    private bool EvalBetween(string aktuellt)
    {
        try
        {
            var range = JsonSerializer.Deserialize<decimal[]>(Varde);
            if (range is not { Length: 2 }) return false;
            return decimal.TryParse(aktuellt, out var val) && val >= range[0] && val <= range[1];
        }
        catch
        {
            return false;
        }
    }
}
