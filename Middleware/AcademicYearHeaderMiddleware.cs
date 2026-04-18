namespace sms.api.Middleware;

public class AcademicYearHeaderMiddleware
{
    public const string HeaderName = "X-Academic-Year-Id";
    public const string HttpContextKey = "AcademicYearId";

    private readonly RequestDelegate _next;

    public AcademicYearHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var value) && !string.IsNullOrWhiteSpace(value)
            && Guid.TryParse(value.ToString().Trim(), out var id))
            context.Items[HttpContextKey] = id;
        await _next(context);
    }
}
