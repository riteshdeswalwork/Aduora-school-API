using GoogleReCaptcha.V3;
using GoogleReCaptcha.V3.Interface;
using Microsoft.Extensions.Options;
using sms.api.Models;
using System.Text.Json.Serialization;

namespace sms.api.Services
{
    public interface IRecaptchaService
    {
        Task<bool> VerifyTokenAsync(string token, string action = "login");
    }

    public class RecaptchaService : IRecaptchaService
    {
        private readonly ICaptchaValidator _captchaValidator;
        private readonly ILogger<RecaptchaService> _logger;
        private readonly double _minimumScore;
        private readonly GoogleReCaptchaConfig _config;

        public RecaptchaService(ICaptchaValidator captchaValidator, ILogger<RecaptchaService> logger, IConfiguration configuration, IOptions<GoogleReCaptchaConfig> config)
        {
            _captchaValidator = captchaValidator;
            _logger = logger;
            _minimumScore = configuration.GetValue<double>("googleReCaptcha:MinimumScore", 0.5);
            _config = config.Value;
        }

        public async Task<bool> VerifyTokenAsync(string token, string action = "login")
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("reCAPTCHA token is empty or null");
                    return false;
                }

                _logger.LogInformation($"Verifying reCAPTCHA token: {token.Substring(0, Math.Min(20, token.Length))}... for action: {action}");
                _logger.LogInformation($"Minimum score required: {_minimumScore}");
                _logger.LogInformation($"Using site key: {_config.SiteKey?.Substring(0, Math.Min(20, _config.SiteKey?.Length ?? 0))}...");
                _logger.LogInformation($"Using secret key: {_config.SecretKey?.Substring(0, Math.Min(20, _config.SecretKey?.Length ?? 0))}...");

                // Manually verify with Google's API
                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify",
                    new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("secret", _config.SecretKey),
                        new KeyValuePair<string, string>("response", token)
                    }));

                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Google reCAPTCHA API response: {jsonResponse}");

                var result = System.Text.Json.JsonSerializer.Deserialize<GoogleReCaptchaResponse>(jsonResponse);

                if (result?.Success == true)
                {
                    _logger.LogInformation($"reCAPTCHA verification successful. Score: {result.Score}, Action: {result.Action}");
                    return result.Score >= _minimumScore;
                }
                else
                {
                    _logger.LogWarning($"reCAPTCHA verification failed. Errors: {string.Join(", ", result?.ErrorCodes ?? new string[0])}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying reCAPTCHA token: {Message}", ex.Message);
                return false;
            }
        }
    }

    public class GoogleReCaptchaResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;

        [JsonPropertyName("challenge_ts")]
        public DateTime ChallengeTs { get; set; }

        [JsonPropertyName("hostname")]
        public string Hostname { get; set; } = string.Empty;

        [JsonPropertyName("error-codes")]
        public string[] ErrorCodes { get; set; } = Array.Empty<string>();
    }
}
