namespace sms.api.Models
{
    public class LiveKitOptions
    {
        public string Url { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public int TokenTtlMinutes { get; set; } = 180;
    }
}
