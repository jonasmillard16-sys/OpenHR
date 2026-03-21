namespace RegionHR.Web.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Content Security Policy
        // - 'unsafe-inline' in script-src: Required by Blazor Server for inline scripts
        //   that bootstrap the SignalR circuit connection (_framework/blazor.server.js injects
        //   inline script elements). Removing this breaks Blazor Server initialization.
        //   A nonce-based approach is not feasible because Blazor dynamically generates
        //   script content during circuit reconnection.
        // - 'unsafe-eval' has been REMOVED: Not required by Blazor Server or MudBlazor.
        // - 'unsafe-inline' in style-src: Required by MudBlazor which applies inline styles
        //   for component positioning, popover placement, and theme variables.
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline'; " +
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
            "font-src 'self' https://fonts.gstatic.com; " +
            "img-src 'self' data:; " +
            "connect-src 'self' ws: wss:;");

        // Other security headers
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");

        // HSTS — instruct browsers to always use HTTPS for this domain.
        // max-age=31536000 = 1 year. includeSubDomains ensures all subdomains also use HTTPS.
        // Only effective when served over HTTPS; browsers ignore it over HTTP.
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

        await _next(context);
    }
}
