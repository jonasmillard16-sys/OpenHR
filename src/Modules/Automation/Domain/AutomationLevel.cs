namespace RegionHR.Automation.Domain;

/// <summary>Automationsnivå — styr hur aggressivt systemet agerar</summary>
public enum AutomationLevel
{
    Notify,     // Bara notifiera användaren
    Suggest,    // Föreslå åtgärd, användaren beslutar
    Autopilot,  // Utför automatiskt
    Block       // Blockera handling som bryter mot regel
}

/// <summary>Status för ett automationsförslag</summary>
public enum SuggestionStatus
{
    Pending,    // Väntar på beslut
    Accepted,   // Accepterad av användare
    Dismissed,  // Avvisad av användare
    Expired     // Giltighetstiden har gått ut
}
