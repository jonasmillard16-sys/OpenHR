namespace RegionHR.Payroll.Engine;

/// <summary>
/// Provides access to system-wide configuration values (e.g. IBB, PBB)
/// stored in the SystemSetting table, without coupling the Payroll module
/// to the Infrastructure layer directly.
/// </summary>
public interface ISystemSettingProvider
{
    /// <summary>
    /// Returns the decimal value for the given setting key, or null if the key
    /// does not exist or its value cannot be parsed as a decimal.
    /// </summary>
    Task<decimal?> GetDecimalAsync(string key, CancellationToken ct = default);
}
