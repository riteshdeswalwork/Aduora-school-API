namespace sms.api.Interfaces.Shared;

public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
    Task SendManyAsync(IEnumerable<string> toEmails, string subject, string htmlBody, CancellationToken ct = default);
}

