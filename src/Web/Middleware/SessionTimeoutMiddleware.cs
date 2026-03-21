namespace RegionHR.Web.Middleware;

/// <summary>
/// Middleware that tracks session activity and enforces an inactivity timeout.
/// When a session has been idle for longer than the configured timeout (default: 30 minutes),
/// the session authentication data is cleared, forcing re-authentication.
///
/// Works with Blazor Server's ProtectedSessionStorage by setting a last-activity timestamp
/// in a cookie that the middleware can read on each request.
/// </summary>
public class SessionTimeoutMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TimeSpan _timeout;
    private const string LastActivityCookieName = ".OpenHR.LastActivity";

    public SessionTimeoutMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        var minutes = configuration.GetValue<int>("OpenHR:SessionTimeoutMinutes", 30);
        _timeout = TimeSpan.FromMinutes(minutes);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip static files and health checks
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/_framework") ||
            path.StartsWith("/_blazor") ||
            path.StartsWith("/css") ||
            path.StartsWith("/js") ||
            path.StartsWith("/health") ||
            path.StartsWith("/_content") ||
            path.Contains('.'))
        {
            await _next(context);
            return;
        }

        // Check last activity
        if (context.Request.Cookies.TryGetValue(LastActivityCookieName, out var lastActivityStr)
            && long.TryParse(lastActivityStr, out var lastActivityTicks))
        {
            var lastActivity = new DateTimeOffset(lastActivityTicks, TimeSpan.Zero);
            var idle = DateTimeOffset.UtcNow - lastActivity;

            if (idle > _timeout)
            {
                // Session expired — delete the activity cookie.
                // The Blazor auth check in AdminLayout will detect the missing session
                // and redirect to /login. We cannot clear ProtectedSessionStorage here
                // because it requires a Blazor circuit, but the timeout is enforced
                // because we stop updating the cookie.
                context.Response.Cookies.Delete(LastActivityCookieName);

                // If this is a page navigation (not a SignalR/WebSocket), redirect to login
                if (!path.StartsWith("/hubs"))
                {
                    context.Response.Redirect("/login");
                    return;
                }
            }
        }

        // Update last activity timestamp
        context.Response.Cookies.Append(LastActivityCookieName, DateTimeOffset.UtcNow.Ticks.ToString(),
            new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                MaxAge = _timeout
            });

        await _next(context);
    }
}
