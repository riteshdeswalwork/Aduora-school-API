using Microsoft.Extensions.Options;
using sms.api.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace sms.api.Services;

public interface ITwilioSmsService
{
    bool IsConfigured { get; }
    Task<SmsSendResult> SendAsync(string toPhone, string body, CancellationToken ct = default);
}

public class SmsSendResult
{
    public bool Success { get; set; }
    public bool Skipped { get; set; }
    public string? MessageId { get; set; }
    public string? Error { get; set; }
}

public class TwilioSmsService : ITwilioSmsService
{
    private readonly TwilioSettings _settings;
    private readonly ILogger<TwilioSmsService> _logger;

    public TwilioSmsService(IOptions<TwilioSettings> settings, ILogger<TwilioSmsService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        IsConfigured = !string.IsNullOrWhiteSpace(_settings.AccountSid)
                       && !string.IsNullOrWhiteSpace(_settings.AuthToken)
                       && !string.IsNullOrWhiteSpace(_settings.FromNumber);

        if (IsConfigured)
        {
            try
            {
                TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Twilio client");
                IsConfigured = false;
            }
        }
        else
        {
            _logger.LogWarning("Twilio is not configured (AccountSid/AuthToken/FromNumber). SMS will be skipped.");
        }
    }

    public bool IsConfigured { get; private set; }

    public async Task<SmsSendResult> SendAsync(string toPhone, string body, CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            return new SmsSendResult { Skipped = true, Error = "Twilio not configured" };
        }

        var phone = NormalizePhone(toPhone);
        if (string.IsNullOrEmpty(phone))
        {
            return new SmsSendResult { Skipped = true, Error = "Invalid phone number" };
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return new SmsSendResult { Skipped = true, Error = "Empty SMS body" };
        }

        try
        {
            // Twilio SDK is sync under the hood; wrap to avoid blocking hot path when queued.
            var message = await Task.Run(() => MessageResource.Create(
                to: new PhoneNumber(phone),
                from: new PhoneNumber(_settings.FromNumber),
                body: TruncateSms(body)
            ), ct);

            return new SmsSendResult
            {
                Success = true,
                MessageId = message.Sid
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Twilio SMS failed to {Phone}", phone);
            return new SmsSendResult { Success = false, Error = ex.Message };
        }
    }

    private static string TruncateSms(string body)
    {
        const int max = 480;
        body = body.Trim();
        return body.Length <= max ? body : body[..(max - 1)] + "…";
    }

    /// <summary>Normalize to E.164-ish; assume India +91 when 10 digits.</summary>
    public static string? NormalizePhone(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (digits.Length == 0) return null;

        if (raw.TrimStart().StartsWith('+'))
        {
            return "+" + digits;
        }

        if (digits.Length == 10)
        {
            return "+91" + digits;
        }

        if (digits.Length == 12 && digits.StartsWith("91"))
        {
            return "+" + digits;
        }

        return digits.StartsWith("91") && digits.Length > 10 ? "+" + digits : "+" + digits;
    }
}
