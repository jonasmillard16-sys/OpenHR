using Microsoft.Extensions.Logging;

namespace RegionHR.Web.Services;

public sealed class ErrorDisplayService
{
    private readonly ILogger<ErrorDisplayService> _logger;

    public ErrorDisplayService(ILogger<ErrorDisplayService> logger)
    {
        _logger = logger;
    }

    public string HandleError(Exception ex, string operation)
    {
        _logger.LogError(ex, "Error during {Operation}", operation);
        return ex switch
        {
            TimeoutException => "Systemet svarar långsamt. Försök igen om en stund.",
            InvalidOperationException ioe when ioe.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
                => "Posten finns redan i systemet. Kontrollera uppgifterna.",
            InvalidOperationException ioe when ioe.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                => "Posten kunde inte hittas. Den kan ha tagits bort.",
            UnauthorizedAccessException => "Du har inte behörighet för denna åtgärd. Kontakta din administratör.",
            ArgumentException => "Felaktiga uppgifter. Kontrollera formuläret och försök igen.",
            _ => "Något gick fel. Försök igen eller kontakta support."
        };
    }

    public async Task<(bool Success, string? Error)> TryAsync(Func<Task> action, string operation)
    {
        try { await action(); return (true, null); }
        catch (Exception ex) { return (false, HandleError(ex, operation)); }
    }
}
