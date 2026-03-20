using RegionHR.Migration.Domain;

namespace RegionHR.Migration.Adapters;

public interface IMigrationAdapter
{
    SourceSystem Source { get; }
    Task<ParsedMigrationData> ParseAsync(Stream fileStream, CancellationToken ct = default);
    MigrationMapping[] GetDefaultMappings();
}

public sealed class ParsedMigrationData
{
    public List<ParsedRecord> Records { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
    public int TotalRows { get; set; }
}

public sealed class ParsedRecord
{
    public string EntityType { get; set; } = string.Empty; // Employee, Employment, PayrollRecord, etc
    public Dictionary<string, string> Fields { get; set; } = [];
}
