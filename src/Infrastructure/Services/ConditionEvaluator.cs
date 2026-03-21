using System.Text.Json;

namespace RegionHR.Infrastructure.Services;

/// <summary>
/// Evaluates JSON condition expressions against an entity context dictionary.
///
/// Supported condition formats:
///   {"field": {">=": 305}}        — comparison operators
///   {"field": true}               — boolean equality
///   {"field": {"=": "value"}}     — string/numeric equality
///   {"field1": ..., "field2": ...} — AND (all conditions must be true)
///   {}                            — empty = always true
///
/// Supported operators: >=, <=, >, <, =
/// </summary>
public sealed class ConditionEvaluator
{
    /// <summary>
    /// Evaluate a JSON condition string against a context dictionary.
    /// Returns true if all conditions are met, or if the condition is empty/null.
    /// </summary>
    public bool Evaluate(string conditionJson, Dictionary<string, object> context)
    {
        if (string.IsNullOrWhiteSpace(conditionJson) || conditionJson == "{}")
            return true;

        JsonElement root;
        try
        {
            root = JsonSerializer.Deserialize<JsonElement>(conditionJson);
        }
        catch (JsonException)
        {
            return false;
        }

        if (root.ValueKind != JsonValueKind.Object)
            return false;

        // All top-level properties are ANDed together
        foreach (var property in root.EnumerateObject())
        {
            if (!EvaluateProperty(property, context))
                return false;
        }

        return true;
    }

    private static bool EvaluateProperty(JsonProperty property, Dictionary<string, object> context)
    {
        var fieldName = property.Name;
        var conditionValue = property.Value;

        // Check if the field exists in context
        if (!context.TryGetValue(fieldName, out var contextValue) || contextValue is null)
            return false;

        // contextValue is guaranteed non-null by the guard above
        var cv = contextValue!;

        return conditionValue.ValueKind switch
        {
            // Boolean condition: {"field": true} or {"field": false}
            JsonValueKind.True => ConvertToBool(cv) == true,
            JsonValueKind.False => ConvertToBool(cv) == false,

            // Numeric direct comparison: {"field": 42}
            JsonValueKind.Number => CompareNumeric(cv, "=", conditionValue.GetDouble()),

            // String direct comparison: {"field": "value"}
            JsonValueKind.String => string.Equals(
                cv.ToString(),
                conditionValue.GetString(),
                StringComparison.OrdinalIgnoreCase),

            // Object with operator(s): {"field": {">=": 305}}
            JsonValueKind.Object => EvaluateOperators(cv, conditionValue),

            _ => false
        };
    }

    private static bool EvaluateOperators(object contextValue, JsonElement operatorObject)
    {
        if (contextValue is null) return false;

        foreach (var op in operatorObject.EnumerateObject())
        {
            var operatorName = op.Name;
            var operandValue = op.Value;

            if (operandValue.ValueKind == JsonValueKind.Number)
            {
                var threshold = operandValue.GetDouble();
                if (!CompareNumeric(contextValue, operatorName, threshold))
                    return false;
            }
            else if (operandValue.ValueKind == JsonValueKind.String)
            {
                if (operatorName == "=")
                {
                    if (!string.Equals(contextValue.ToString(), operandValue.GetString(), StringComparison.OrdinalIgnoreCase))
                        return false;
                }
                else
                {
                    return false; // Non-equality operators don't apply to strings
                }
            }
            else if (operandValue.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                if (operatorName == "=")
                {
                    if (ConvertToBool(contextValue) != operandValue.GetBoolean())
                        return false;
                }
                else
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool CompareNumeric(object contextValue, string op, double threshold)
    {
        var contextNum = ConvertToDouble(contextValue);
        if (contextNum is null)
            return false;

        return op switch
        {
            ">=" => contextNum.Value >= threshold,
            "<=" => contextNum.Value <= threshold,
            ">" => contextNum.Value > threshold,
            "<" => contextNum.Value < threshold,
            "=" or "==" => Math.Abs(contextNum.Value - threshold) < 0.0001,
            "!=" => Math.Abs(contextNum.Value - threshold) >= 0.0001,
            _ => false
        };
    }

    private static double? ConvertToDouble(object value)
    {
        return value switch
        {
            int i => i,
            long l => l,
            float f => f,
            double d => d,
            decimal m => (double)m,
            string s when double.TryParse(s, out var r) => r,
            _ => null
        };
    }

    private static bool? ConvertToBool(object value)
    {
        return value switch
        {
            bool b => b,
            string s when bool.TryParse(s, out var r) => r,
            int i => i != 0,
            _ => null
        };
    }
}
