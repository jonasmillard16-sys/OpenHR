namespace RegionHR.Migration.Domain;

/// <summary>Status för ett migreringsjobb</summary>
public enum MigrationJobStatus
{
    Created,
    Validating,
    DryRun,
    Importing,
    Complete,
    Failed
}

/// <summary>Källsystem att migrera från</summary>
public enum SourceSystem
{
    PAXml,
    HEROMA,
    PersonecP,
    Hogia,
    Fortnox,
    SIE4i,
    Workday,
    SAP,
    OracleHCM,
    GenericCSV
}

/// <summary>Status för en enskild importpost</summary>
public enum MigrationLogStatus
{
    Success,
    Error
}
