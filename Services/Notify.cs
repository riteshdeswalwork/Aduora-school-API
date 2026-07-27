using sms.api.Services;

namespace sms.api.Services;

/// <summary>Small helpers so managers can fire notifications without ceremony.</summary>
public static class Notify
{
    public static void Fire(INotificationOrchestrator? orch, NotificationRequest request)
    {
        if (orch == null || request == null) return;
        _ = Task.Run(async () =>
        {
            try
            {
                await orch.NotifyAsync(request);
            }
            catch
            {
                // Orchestrator logs internally; never fail domain flow
            }
        });
    }
}
