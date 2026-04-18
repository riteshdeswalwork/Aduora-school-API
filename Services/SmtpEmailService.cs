using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Services;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        return SendManyAsync(new[] { toEmail }, subject, htmlBody, ct);
    }

    public async Task SendManyAsync(IEnumerable<string> toEmails, string subject, string htmlBody, CancellationToken ct = default)
    {
        var recipients = (toEmails ?? Array.Empty<string>()).Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (recipients.Count == 0) return;

        if (string.IsNullOrWhiteSpace(_settings.SmtpHost) || string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            throw new InvalidOperationException("Email settings are not configured (SmtpHost/FromEmail).");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject ?? "",
            Body = htmlBody ?? "",
            IsBodyHtml = true
        };

        foreach (var r in recipients)
        {
            message.To.Add(r);
        }

        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(_settings.SmtpUsername))
        {
            client.Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword);
        }

        try
        {
            await client.SendMailAsync(message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via SMTP");
            throw;
        }
    }
}

