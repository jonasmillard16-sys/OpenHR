using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RegionHR.Infrastructure.Notifications;

public class EmailNotificationSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailNotificationSender> _logger;

    public EmailNotificationSender(IConfiguration config, ILogger<EmailNotificationSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string bodyHtml, CancellationToken ct = default)
    {
        var smtpHost = _config["Email:SmtpHost"] ?? "localhost";
        var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "25");
        var fromEmail = _config["Email:FromEmail"] ?? "noreply@openhr.se";
        var fromName = _config["Email:FromName"] ?? "OpenHR";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = WrapInTemplate(subject, bodyHtml)
        };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, false, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
            _logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email to {Email}: {Subject}", toEmail, subject);
            // Don't throw — email failures should not break workflows
        }
    }

    private static string WrapInTemplate(string title, string content) =>
        "<!DOCTYPE html>" +
        "<html lang=\"sv\">" +
        "<head><meta charset=\"utf-8\" /><style>" +
        "body { font-family: 'Segoe UI', sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; color: #1c2833; }" +
        ".header { background: #1a5276; color: white; padding: 20px; border-radius: 8px 8px 0 0; }" +
        ".content { padding: 20px; border: 1px solid #d5d8dc; border-top: 0; border-radius: 0 0 8px 8px; }" +
        ".footer { padding: 15px; text-align: center; color: #5d6d7e; font-size: 0.875rem; }" +
        "</style></head>" +
        "<body>" +
        "<div class=\"header\"><h2>OpenHR</h2></div>" +
        $"<div class=\"content\"><h3>{title}</h3>{content}</div>" +
        "<div class=\"footer\">Skickat fr\u00e5n OpenHR \u2014 Fritt HR-system</div>" +
        "</body>" +
        "</html>";

    // Convenience methods for common notifications
    public Task SendApprovalRequestAsync(string toEmail, string toName, string requesterName, string requestType, CancellationToken ct = default)
        => SendAsync(toEmail, toName, $"Godkännande krävs: {requestType}",
            $"<p>{requesterName} har skickat in en förfrågan om <strong>{requestType}</strong> som väntar på ditt godkännande.</p><p>Logga in i OpenHR för att granska och godkänna.</p>", ct);

    public Task SendApprovalResultAsync(string toEmail, string toName, string requestType, bool approved, CancellationToken ct = default)
        => SendAsync(toEmail, toName, $"{requestType} — {(approved ? "Godkänd" : "Avslagen")}",
            $"<p>Din förfrågan om <strong>{requestType}</strong> har {(approved ? "godkänts" : "avslagits")}.</p><p>Logga in i OpenHR för mer information.</p>", ct);

    public Task SendSickLeaveReminderAsync(string toEmail, string toName, int sickDay, CancellationToken ct = default)
        => SendAsync(toEmail, toName, $"Påminnelse: Läkarintyg krävs (sjukdag {sickDay})",
            $"<p>Du har varit sjuk i {sickDay} dagar. Från och med dag 8 krävs ett läkarintyg.</p><p>Ladda upp ditt läkarintyg i OpenHR under Mina dokument.</p>", ct);
}
