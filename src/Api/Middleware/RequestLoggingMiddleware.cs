namespace RegionHR.Api.Middleware;

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
        }
        finally
        {
            var elapsed = DateTime.UtcNow - start;
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                _logger.LogInformation(
                    "{Method} {Path} {StatusCode} {Elapsed}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsed.TotalMilliseconds);
            }
        }
    }
}
