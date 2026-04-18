namespace sms.api.Interfaces.Shared
{
    public interface ILiveKitTokenService
    {
        string BuildParticipantToken(string identity, string participantName, string roomName, bool canPublish = true, bool canSubscribe = true);
    }
}
