namespace RegionHR.Infrastructure.Services;

/// <summary>
/// Thrown when an automation rule at Block level prevents a triggering action.
/// </summary>
public sealed class AutomationBlockException : Exception
{
    public string RuleName { get; }

    public AutomationBlockException(string ruleName, string message)
        : base(message)
    {
        RuleName = ruleName;
    }
}
