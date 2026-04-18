using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using sms.api.Interfaces.Shared;
using sms.api.Models;

namespace sms.api.Services
{
    public class LiveKitTokenService : ILiveKitTokenService
    {
        private readonly LiveKitOptions _options;

        public LiveKitTokenService(IOptions<LiveKitOptions> options)
        {
            _options = options.Value;
        }

        public string BuildParticipantToken(string identity, string participantName, string roomName, bool canPublish = true, bool canSubscribe = true)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.ApiSecret))
            {
                throw new InvalidOperationException("LiveKit API credentials are not configured.");
            }

            if (string.IsNullOrWhiteSpace(roomName))
            {
                throw new ArgumentException("Room name is required.", nameof(roomName));
            }

            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(_options.TokenTtlMinutes <= 0 ? 180 : _options.TokenTtlMinutes);

            var videoGrantJson =
                $"{{\"roomJoin\":true,\"room\":\"{EscapeJson(roomName)}\",\"canPublish\":{canPublish.ToString().ToLowerInvariant()},\"canSubscribe\":{canSubscribe.ToString().ToLowerInvariant()},\"canPublishData\":true}}";

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, identity),
                new("name", participantName),
                new("video", videoGrantJson, JsonClaimValueTypes.Json)
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.ApiSecret));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.ApiKey,
                audience: null,
                claims: claims,
                notBefore: now.AddMinutes(-1),
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string EscapeJson(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
