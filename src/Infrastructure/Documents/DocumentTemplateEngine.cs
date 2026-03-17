namespace RegionHR.Infrastructure.Documents;

/// <summary>
/// Generates documents from templates using merge fields.
/// Templates use {{FieldName}} syntax for placeholders.
/// </summary>
public class DocumentTemplateEngine
{
    private static readonly Dictionary<string, string> _templates = new()
    {
        ["anstallningsavtal"] = """
            ANSTÄLLNINGSAVTAL

            Arbetsgivare: {{Arbetsgivare}}
            Organisationsnummer: {{OrgNummer}}

            Anställd: {{Fornamn}} {{Efternamn}}
            Personnummer: {{Personnummer}}

            Befattning: {{Befattning}}
            Organisationsenhet: {{Enhet}}
            Anställningsform: {{Anstallningsform}}
            Sysselsättningsgrad: {{Sysselsattningsgrad}}%
            Månadslön: {{Manadslon}} kr
            Kollektivavtal: {{Kollektivavtal}}

            Tillträdesdatum: {{Startdatum}}
            {{#if Slutdatum}}Anställningen upphör: {{Slutdatum}}{{/if}}

            Övriga villkor:
            - Arbetstid enligt gällande kollektivavtal
            - Semester enligt Semesterlagen och kollektivavtal
            - Pension enligt AKAP-KR
            - Ömsesidig uppsägningstid enligt LAS och kollektivavtal

            Ort och datum: _______________

            Arbetsgivare: _______________     Anställd: _______________
            """,

        ["tjanstgoringsbevis"] = """
            TJÄNSTGÖRINGSINTYG

            Härmed intygas att {{Fornamn}} {{Efternamn}}, personnummer {{Personnummer}},
            har varit anställd hos {{Arbetsgivare}} under perioden
            {{Startdatum}} — {{Slutdatum}}.

            Befattning: {{Befattning}}
            Organisationsenhet: {{Enhet}}
            Sysselsättningsgrad: {{Sysselsattningsgrad}}%

            {{Arbetsgivare}}, {{Datum}}

            _______________
            {{Utfardare}}
            HR-avdelningen
            """,

        ["loneandring"] = """
            BEKRÄFTELSE PÅ LÖNEÄNDRING

            Anställd: {{Fornamn}} {{Efternamn}}
            Personnummer: {{Personnummer}}

            Tidigare månadslön: {{TidigareLon}} kr
            Ny månadslön: {{NyLon}} kr
            Förändring: {{Forandring}} kr ({{ForandringProcent}}%)

            Gäller från: {{GallerFran}}
            Anledning: {{Anledning}}

            {{Arbetsgivare}}, {{Datum}}
            """,
    };

    public string Generate(string templateName, Dictionary<string, string> fields)
    {
        if (!_templates.TryGetValue(templateName, out var template))
            throw new ArgumentException($"Mall '{templateName}' finns inte.");

        var result = template;
        foreach (var (key, value) in fields)
        {
            result = result.Replace($"{{{{{key}}}}}", value);
        }

        // Handle conditional blocks {{#if Field}}...{{/if}}
        result = System.Text.RegularExpressions.Regex.Replace(result,
            @"\{\{#if (\w+)\}\}(.*?)\{\{/if\}\}",
            m => fields.ContainsKey(m.Groups[1].Value) && !string.IsNullOrEmpty(fields.GetValueOrDefault(m.Groups[1].Value))
                ? m.Groups[2].Value : "",
            System.Text.RegularExpressions.RegexOptions.Singleline);

        // Clean up any remaining unfilled fields
        result = System.Text.RegularExpressions.Regex.Replace(result, @"\{\{.*?\}\}", "");

        return result.Trim();
    }

    public List<string> GetTemplateNames() => [.. _templates.Keys];

    public List<string> GetFieldNames(string templateName)
    {
        if (!_templates.TryGetValue(templateName, out var template))
            return [];
        return System.Text.RegularExpressions.Regex.Matches(template, @"\{\{(\w+)\}\}")
            .Select(m => m.Groups[1].Value)
            .Where(f => f != "#if" && f != "/if")
            .Distinct()
            .ToList();
    }
}
