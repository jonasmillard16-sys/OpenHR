namespace RegionHR.SharedKernel.Abstractions;

/// <summary>
/// Domain exception that carries a localizable error code.
/// The UI layer maps error codes to localized messages via IStringLocalizer.
/// </summary>
public class DomainException : InvalidOperationException
{
    /// <summary>
    /// A machine-readable error code (e.g. "CASE_ALREADY_APPROVED") that can be
    /// mapped to a localized string in SharedResources.resx at the UI layer.
    /// </summary>
    public string ErrorCode { get; }

    public DomainException(string errorCode, string? message = null)
        : base(message ?? errorCode)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string errorCode, string? message, Exception? innerException)
        : base(message ?? errorCode, innerException)
    {
        ErrorCode = errorCode;
    }
}
