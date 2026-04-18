using System.Collections.Generic;

namespace sms.api.Interfaces.Shared;

public interface ITrafficMetricsService
{
    void MarkRequestStarted();
    void MarkRequestCompleted(string path, int statusCode, double durationMs);
    object GetSnapshot();
}
