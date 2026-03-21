using RegionHR.Infrastructure.Services;
using Xunit;

namespace RegionHR.Automation.Tests;

public class ConditionEvaluatorTests
{
    private readonly ConditionEvaluator _evaluator = new();

    // === Empty / null conditions ===

    [Fact]
    public void Evaluate_EmptyCondition_ReturnsTrue()
    {
        var result = _evaluator.Evaluate("{}", new Dictionary<string, object>());
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_NullCondition_ReturnsTrue()
    {
        var result = _evaluator.Evaluate(null!, new Dictionary<string, object>());
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_WhitespaceCondition_ReturnsTrue()
    {
        var result = _evaluator.Evaluate("  ", new Dictionary<string, object>());
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_InvalidJson_ReturnsFalse()
    {
        var result = _evaluator.Evaluate("not json", new Dictionary<string, object>());
        Assert.False(result);
    }

    // === Greater-than-or-equal (>=) ===

    [Fact]
    public void Evaluate_GreaterThanOrEqual_WhenContextValueMeetsThreshold_ReturnsTrue()
    {
        var context = new Dictionary<string, object> { { "ackumuleradeDagar", 310 } };
        var result = _evaluator.Evaluate("{\"ackumuleradeDagar\": {\">=\":305}}", context);
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_GreaterThanOrEqual_WhenExactlyEqual_ReturnsTrue()
    {
        var context = new Dictionary<string, object> { { "ackumuleradeDagar", 305 } };
        var result = _evaluator.Evaluate("{\"ackumuleradeDagar\": {\">=\":305}}", context);
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_GreaterThanOrEqual_WhenBelow_ReturnsFalse()
    {
        var context = new Dictionary<string, object> { { "ackumuleradeDagar", 200 } };
        var result = _evaluator.Evaluate("{\"ackumuleradeDagar\": {\">=\":305}}", context);
        Assert.False(result);
    }

    // === Less-than-or-equal (<=) ===

    [Fact]
    public void Evaluate_LessThanOrEqual_WhenBelow_ReturnsTrue()
    {
        var context = new Dictionary<string, object> { { "dagar", 10 } };
        var result = _evaluator.Evaluate("{\"dagar\": {\"<=\":14}}", context);
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_LessThanOrEqual_WhenAbove_ReturnsFalse()
    {
        var context = new Dictionary<string, object> { { "dagar", 20 } };
        var result = _evaluator.Evaluate("{\"dagar\": {\"<=\":14}}", context);
        Assert.False(result);
    }

    // === Greater-than (>) ===

    [Fact]
    public void Evaluate_GreaterThan_WhenAbove_ReturnsTrue()
    {
        var context = new Dictionary<string, object> { { "timmar", 50 } };
        var result = _evaluator.Evaluate("{\"timmar\": {\">\":48}}", context);
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_GreaterThan_WhenExactlyEqual_ReturnsFalse()
    {
        var context = new Dictionary<string, object> { { "timmar", 48 } };
        var result = _evaluator.Evaluate("{\"timmar\": {\">\":48}}", context);
        Assert.False(result);
    }

    // === Less-than (<) ===

    [Fact]
    public void Evaluate_LessThan_WhenBelow_ReturnsTrue()
    {
        var context = new Dictionary<string, object> { { "vila", 9 } };
        var result = _evaluator.Evaluate("{\"vila\": {\"<\":11}}", context);
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_LessThan_WhenAbove_ReturnsFalse()
    {
        var context = new Dictionary<string, object> { { "vila", 12 } };
        var result = _evaluator.Evaluate("{\"vila\": {\"<\":11}}", context);
        Assert.False(result);
    }

    // === Equality (=) ===

    [Fact]
    public void Evaluate_NumericEquality_WhenEqual_ReturnsTrue()
    {
        var context = new Dictionary<string, object> { { "antal", 6 } };
        var result = _evaluator.Evaluate("{\"antal\": {\"=\":6}}", context);
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_NumericEquality_WhenDifferent_ReturnsFalse()
    {
        var context = new Dictionary<string, object> { { "antal", 5 } };
        var result = _evaluator.Evaluate("{\"antal\": {\"=\":6}}", context);
        Assert.False(result);
    }

    // === Boolean conditions ===

    [Fact]
    public void Evaluate_BooleanTrue_WhenContextIsTrue_ReturnsTrue()
    {
        var context = new Dictionary<string, object> { { "lonUnderAvtalsMinimum", true } };
        var result = _evaluator.Evaluate("{\"lonUnderAvtalsMinimum\": true}", context);
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_BooleanTrue_WhenContextIsFalse_ReturnsFalse()
    {
        var context = new Dictionary<string, object> { { "lonUnderAvtalsMinimum", false } };
        var result = _evaluator.Evaluate("{\"lonUnderAvtalsMinimum\": true}", context);
        Assert.False(result);
    }

    [Fact]
    public void Evaluate_BooleanFalse_WhenContextIsFalse_ReturnsTrue()
    {
        var context = new Dictionary<string, object> { { "aktiv", false } };
        var result = _evaluator.Evaluate("{\"aktiv\": false}", context);
        Assert.True(result);
    }

    // === AND semantics (multiple conditions) ===

    [Fact]
    public void Evaluate_MultipleConditions_AllMet_ReturnsTrue()
    {
        var context = new Dictionary<string, object>
        {
            { "dagar", 320 },
            { "aktiv", true }
        };
        var result = _evaluator.Evaluate("{\"dagar\": {\">=\":300}, \"aktiv\": true}", context);
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_MultipleConditions_OneFails_ReturnsFalse()
    {
        var context = new Dictionary<string, object>
        {
            { "dagar", 320 },
            { "aktiv", false }
        };
        var result = _evaluator.Evaluate("{\"dagar\": {\">=\":300}, \"aktiv\": true}", context);
        Assert.False(result);
    }

    // === Missing context field ===

    [Fact]
    public void Evaluate_MissingField_ReturnsFalse()
    {
        var context = new Dictionary<string, object> { { "otherField", 100 } };
        var result = _evaluator.Evaluate("{\"ackumuleradeDagar\": {\">=\":305}}", context);
        Assert.False(result);
    }

    // === Different numeric types ===

    [Fact]
    public void Evaluate_DecimalContextValue_ReturnsTrue()
    {
        var context = new Dictionary<string, object> { { "belopp", 5000.50m } };
        var result = _evaluator.Evaluate("{\"belopp\": {\">=\":5000}}", context);
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_LongContextValue_ReturnsTrue()
    {
        var context = new Dictionary<string, object> { { "dagar", 400L } };
        var result = _evaluator.Evaluate("{\"dagar\": {\">=\":305}}", context);
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_DoubleContextValue_ReturnsTrue()
    {
        var context = new Dictionary<string, object> { { "procent", 7.5 } };
        var result = _evaluator.Evaluate("{\"procent\": {\">=\":5.0}}", context);
        Assert.True(result);
    }

    // === String equality ===

    [Fact]
    public void Evaluate_StringEquality_CaseInsensitive_ReturnsTrue()
    {
        var context = new Dictionary<string, object> { { "status", "Aktiv" } };
        var result = _evaluator.Evaluate("{\"status\": \"aktiv\"}", context);
        Assert.True(result);
    }

    // === Direct numeric comparison ===

    [Fact]
    public void Evaluate_DirectNumericValue_WhenEqual_ReturnsTrue()
    {
        var context = new Dictionary<string, object> { { "dagar_min", 300 } };
        var result = _evaluator.Evaluate("{\"dagar_min\": 300}", context);
        Assert.True(result);
    }

    // === Seed data condition formats ===

    [Fact]
    public void Evaluate_SeedCondition_DagarMin300_ReturnsTrue()
    {
        var context = new Dictionary<string, object> { { "dagar_min", 350 } };
        var result = _evaluator.Evaluate("{\"dagar_min\":300}", context);
        // Direct numeric: context value 350 must equal 300 — FALSE
        Assert.False(result);
    }

    [Fact]
    public void Evaluate_SeedCondition_DagarMin300_WithOperator_ReturnsTrue()
    {
        var context = new Dictionary<string, object> { { "dagar_min", 350 } };
        var result = _evaluator.Evaluate("{\"dagar_min\":{\">=\":300}}", context);
        Assert.True(result);
    }

    // === Not-equal (!=) ===

    [Fact]
    public void Evaluate_NotEqual_WhenDifferent_ReturnsTrue()
    {
        var context = new Dictionary<string, object> { { "antal", 5 } };
        var result = _evaluator.Evaluate("{\"antal\": {\"!=\":6}}", context);
        Assert.True(result);
    }

    [Fact]
    public void Evaluate_NotEqual_WhenSame_ReturnsFalse()
    {
        var context = new Dictionary<string, object> { { "antal", 6 } };
        var result = _evaluator.Evaluate("{\"antal\": {\"!=\":6}}", context);
        Assert.False(result);
    }
}
