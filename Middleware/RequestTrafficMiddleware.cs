using System.Diagnostics;
using sms.api.Interfaces.Shared;

namespace sms.api.Middleware;

public class RequestTrafficMiddleware
{
    private readonly RequestDelegate _next;

    public RequestTrafficMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITrafficMetricsService trafficMetrics)
    {
        var path = context.Request.Path.Value ?? "/";
        var shouldTrack =
            path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/onlineclasshub", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/chathub", StringComparison.OrdinalIgnoreCase);

        Stopwatch? sw = null;
        if (shouldTrack)
        {
            trafficMetrics.MarkRequestStarted();
            sw = Stopwatch.StartNew();
        }

        try
        {
            await _next(context);
        }
        finally
        {
            if (shouldTrack && sw is not null)
            {
                sw.Stop();
                trafficMetrics.MarkRequestCompleted(path, context.Response.StatusCode, sw.Elapsed.TotalMilliseconds);
            }
        }
    }
}
