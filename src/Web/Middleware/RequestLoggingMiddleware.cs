namespace RegionHR.Web.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var start = DateTime.UtcNow;
        try
        {
            await _next(context);
            var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
            if (!context.Request.Path.StartsWithSegments("/_blazor") &&
                !context.Request.Path.StartsWithSegments("/_framework"))
            {
                _logger.LogInformation("HTTP {Method} {Path} → {StatusCode} ({Elapsed:F0}ms)",
                    context.Request.Method, context.Request.Path, context.Response.StatusCode, elapsed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP {Method} {Path} → 500", context.Request.Method, context.Request.Path);
            throw;
        }
    }
}
