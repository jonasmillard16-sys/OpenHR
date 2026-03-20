namespace RegionHR.Compensation.Domain;

/// <summary>
/// Rorlig lonekomponent (provision, jour, beredskap).
/// </summary>
public sealed class VariablePayComponent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Namn { get; set; } = string.Empty;
    public VariablePayTyp Typ { get; set; }
    public string? BerakningsRegel { get; set; }  // JSON
    public bool KoppladTillTiddata { get; set; }
}

public enum VariablePayTyp
{
    Provision,
    Jour,
    Beredskap
}
