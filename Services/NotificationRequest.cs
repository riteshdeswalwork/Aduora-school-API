namespace sms.api.Services;

[Flags]
public enum NotificationChannels
{
    None = 0,
    InApp = 1,
    Push = 2,
    Email = 4,
    Sms = 8,
    All = InApp | Push | Email | Sms
}

public class NotificationRequest
{
    public List<Guid> UserIds { get; set; } = new();

    /// <summary>When true, also include configured admin recipients (users/emails/phones).</summary>
    public bool IncludeAdmins { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    /// <summary>Short SMS body; falls back to Body truncated if empty.</summary>
    public string? SmsBody { get; set; }

    public string? EmailSubject { get; set; }
    public string? EmailHtmlBody { get; set; }

    public string Type { get; set; } = "general";
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string Severity { get; set; } = "info";

    public NotificationChannels Channels { get; set; } = NotificationChannels.All;

    public Dictionary<string, string>? Data { get; set; }

    /// <summary>When set, skip duplicate delivery for same user+type+key within a short window.</summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>Category for preference mute flags: approvals, fees, attendance, system, general</summary>
    public string Category { get; set; } = "general";
}

public class NotificationDispatchResult
{
    public int RecipientCount { get; set; }
    public int InAppCreated { get; set; }
    public int PushQueued { get; set; }
    public int EmailQueued { get; set; }
    public int SmsQueued { get; set; }
}
