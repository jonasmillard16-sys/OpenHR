using System.Text.Json;
using RegionHR.Configuration.Domain;

namespace RegionHR.Infrastructure.Services;

/// <summary>
/// Validerar CustomObjectRecord-data mot CustomObject.FaltSchema.
/// FaltSchema är ett JSON-array med fältdefinitioner.
/// </summary>
public class CustomObjectValidator
{
    /// <summary>
    /// Validerar postdata mot objektdefinitionens fältschema.
    /// Returnerar en lista med valideringsfel på svenska.
    /// </summary>
    public List<string> Validera(CustomObject definition, string recordDataJson)
    {
        var fel = new List<string>();

        // Parsa postdata
        JsonElement recordData;
        try
        {
            using var dataDoc = JsonDocument.Parse(recordDataJson);
            recordData = dataDoc.RootElement.Clone();
        }
        catch (JsonException)
        {
            fel.Add("Postdata är ogiltig JSON.");
            return fel;
        }

        if (recordData.ValueKind != JsonValueKind.Object)
        {
            fel.Add("Postdata måste vara ett JSON-objekt.");
            return fel;
        }

        // Parsa fältschema
        List<FaltDefinition> faltDefinitioner;
        try
        {
            faltDefinitioner = ParseaFaltSchema(definition.FaltSchema);
        }
        catch (Exception ex)
        {
            fel.Add($"Fältschemat är ogiltigt: {ex.Message}");
            return fel;
        }

        foreach (var falt in faltDefinitioner)
        {
            var harFalt = recordData.TryGetProperty(falt.Namn, out var varde);

            // Kontrollera obligatoriska fält
            if (falt.Obligatorisk)
            {
                if (!harFalt || varde.ValueKind == JsonValueKind.Null ||
                    (varde.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(varde.GetString())))
                {
                    fel.Add($"Fältet '{falt.Namn}' är obligatoriskt.");
                    continue;
                }
            }

            if (!harFalt || varde.ValueKind == JsonValueKind.Null)
                continue;

            // Typvalidering
            var typFel = ValideraTyp(falt, varde);
            if (typFel is not null)
                fel.Add(typFel);
        }

        return fel;
    }

    private static string? ValideraTyp(FaltDefinition falt, JsonElement varde)
    {
        return falt.Typ switch
        {
            CustomObjectFieldType.Text or
            CustomObjectFieldType.Dropdown or
            CustomObjectFieldType.MultiSelect =>
                varde.ValueKind != JsonValueKind.String
                    ? $"Fältet '{falt.Namn}' måste vara text."
                    : null,

            CustomObjectFieldType.Number =>
                varde.ValueKind != JsonValueKind.Number
                    ? $"Fältet '{falt.Namn}' måste vara ett tal."
                    : null,

            CustomObjectFieldType.Date =>
                ValidateDatum(falt.Namn, varde),

            CustomObjectFieldType.YesNo =>
                varde.ValueKind != JsonValueKind.True && varde.ValueKind != JsonValueKind.False
                    ? $"Fältet '{falt.Namn}' måste vara ett ja/nej-värde (true/false)."
                    : null,

            CustomObjectFieldType.Email =>
                ValidateEpost(falt.Namn, varde),

            CustomObjectFieldType.Phone =>
                varde.ValueKind != JsonValueKind.String
                    ? $"Fältet '{falt.Namn}' måste vara text (telefonnummer)."
                    : null,

            CustomObjectFieldType.URL =>
                ValidateUrl(falt.Namn, varde),

            _ => null
        };
    }

    private static string? ValidateDatum(string namn, JsonElement varde)
    {
        if (varde.ValueKind != JsonValueKind.String)
            return $"Fältet '{namn}' måste vara en datumsträng (YYYY-MM-DD).";

        var str = varde.GetString() ?? "";
        if (!DateOnly.TryParseExact(str, "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out _))
        {
            return $"Fältet '{namn}' har ogiltigt datumformat. Förväntat format: YYYY-MM-DD.";
        }

        return null;
    }

    private static string? ValidateEpost(string namn, JsonElement varde)
    {
        if (varde.ValueKind != JsonValueKind.String)
            return $"Fältet '{namn}' måste vara text (e-postadress).";

        var str = varde.GetString() ?? "";
        if (!str.Contains('@') || !str.Contains('.'))
            return $"Fältet '{namn}' är inte en giltig e-postadress.";

        return null;
    }

    private static string? ValidateUrl(string namn, JsonElement varde)
    {
        if (varde.ValueKind != JsonValueKind.String)
            return $"Fältet '{namn}' måste vara text (URL).";

        var str = varde.GetString() ?? "";
        if (!Uri.TryCreate(str, UriKind.Absolute, out _))
            return $"Fältet '{namn}' är inte en giltig URL.";

        return null;
    }

    private static List<FaltDefinition> ParseaFaltSchema(string faltSchemaJson)
    {
        if (string.IsNullOrWhiteSpace(faltSchemaJson) || faltSchemaJson == "[]")
            return [];

        using var doc = JsonDocument.Parse(faltSchemaJson);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException("FaltSchema måste vara ett JSON-array.");

        var result = new List<FaltDefinition>();
        foreach (var element in root.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object) continue;

            var namn = element.TryGetProperty("namn", out var namnEl) ? namnEl.GetString() ?? "" :
                       element.TryGetProperty("name", out var nameEl) ? nameEl.GetString() ?? "" : "";

            var typ = element.TryGetProperty("typ", out var typEl) ? typEl.GetString() ?? CustomObjectFieldType.Text :
                      element.TryGetProperty("type", out var typeEl) ? typeEl.GetString() ?? CustomObjectFieldType.Text :
                      CustomObjectFieldType.Text;

            var obligatorisk = element.TryGetProperty("obligatorisk", out var oblEl) ? oblEl.GetBoolean() :
                               element.TryGetProperty("required", out var reqEl) && reqEl.GetBoolean();

            if (!string.IsNullOrWhiteSpace(namn))
                result.Add(new FaltDefinition(namn, typ, obligatorisk));
        }

        return result;
    }

    private record FaltDefinition(string Namn, string Typ, bool Obligatorisk);
}
