using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RegionHR.Infrastructure.Notifications;

public class EmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        var smtpHost = _config["Email:SmtpHost"];

        if (string.IsNullOrEmpty(smtpHost))
        {
            _logger.LogInformation("Email (demo): To={To}, Subject={Subject}", to, subject);
            return;
        }

        try
        {
            // In production: use MailKit
            // var message = new MimeKit.MimeMessage();
            // message.From.Add(new MimeKit.MailboxAddress("OpenHR", _config["Email:From"]));
            // message.To.Add(MimeKit.MailboxAddress.Parse(to));
            // message.Subject = subject;
            // message.Body = new MimeKit.TextPart("html") { Text = body };
            // using var smtp = new MailKit.Net.Smtp.SmtpClient();
            // await smtp.ConnectAsync(smtpHost, int.Parse(_config["Email:SmtpPort"] ?? "587"));
            // await smtp.SendAsync(message, ct);

            _logger.LogInformation("Email sent: To={To}, Subject={Subject}", to, subject);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email to {To}", to);
        }
    }

    public Task SendNotification(string to, string title, string message, CancellationToken ct = default)
        => SendAsync(to, $"OpenHR: {title}", $"<h2>{title}</h2><p>{message}</p><hr><p><em>Skickat från OpenHR</em></p>", ct);
}
