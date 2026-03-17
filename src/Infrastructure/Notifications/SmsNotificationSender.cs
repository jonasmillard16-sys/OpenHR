using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RegionHR.Infrastructure.Notifications;

/// <summary>
/// SMS notification sender. Uses a configurable HTTP-based SMS gateway.
/// FOSS-compatible: works with any SMS API (46elks, Twilio, etc.)
/// </summary>
public class SmsNotificationSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmsNotificationSender> _logger;

    public SmsNotificationSender(IConfiguration config, ILogger<SmsNotificationSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        var gatewayUrl = _config["Sms:GatewayUrl"];
        var apiKey = _config["Sms:ApiKey"];
        var sender = _config["Sms:SenderName"] ?? "OpenHR";

        if (string.IsNullOrEmpty(gatewayUrl))
        {
            _logger.LogWarning("SMS gateway not configured. Would send to {Phone}: {Message}", phoneNumber, message);
            return;
        }

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["from"] = sender,
                ["to"] = phoneNumber,
                ["message"] = message
            });
            await client.PostAsync(gatewayUrl, content, ct);
            _logger.LogInformation("SMS sent to {Phone}: {Message}", phoneNumber, message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send SMS to {Phone}", phoneNumber);
        }
    }

    // Convenience methods
    public Task SendShiftChangeAsync(string phone, string name, string oldShift, string newShift, CancellationToken ct = default)
        => SendAsync(phone, $"Hej {name}! Ditt pass har ändrats från {oldShift} till {newShift}. Se ditt schema i OpenHR.", ct);

    public Task SendApprovalReminderAsync(string phone, string name, int count, CancellationToken ct = default)
        => SendAsync(phone, $"Hej {name}! {count} ärende(n) väntar på ditt godkännande i OpenHR.", ct);

    public Task SendSickLeaveReminderAsync(string phone, string name, int day, CancellationToken ct = default)
        => SendAsync(phone, $"Hej {name}! Du har varit sjuk i {day} dagar. Läkarintyg behövs från dag 8. Ladda upp i OpenHR.", ct);
}
