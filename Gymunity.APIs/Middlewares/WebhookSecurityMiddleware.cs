// ITI.Gymunity.FP.APIs/Middleware/WebhookSecurityMiddleware.cs
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace Gymunity.APIs.Middlewares
{
    /// <summary>
    /// Middleware responsible for securing webhook endpoints through rate limiting, 
    /// IP whitelisting, and request logging.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="WebhookSecurityMiddleware"/> class.
    /// </remarks>
    /// <param name="next">The next delegate in the request pipeline.</param>
    /// <param name="logger">The logger instance for security auditing.</param>
    /// <param name="cache">Memory cache for temporary data storage.</param>
    /// <param name="configuration">Application configuration for retrieving security settings.</param>
    public class WebhookSecurityMiddleware(
        RequestDelegate next,
        ILogger<WebhookSecurityMiddleware> logger,
        IMemoryCache cache,
        IConfiguration configuration)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<WebhookSecurityMiddleware> _logger = logger;
        private readonly IMemoryCache _cache = cache;
        private readonly IConfiguration _configuration = configuration;

        // Rate limiting: Track requests per IP
        private static readonly ConcurrentDictionary<string, Queue<DateTime>> _requestTracker = new();

        /// <summary>
        /// Invokes the middleware logic to validate the incoming request security.
        /// </summary>
        /// <param name="context">The current <see cref="HttpContext"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // Only apply to webhook endpoints
            if (context.Request.Path.StartsWithSegments("/api/webhooks"))
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                _logger.LogInformation(
                    "🔒 Webhook request | Method: {Method} | Path: {Path} | IP: {IP}",
                    context.Request.Method,
                    context.Request.Path,
                    ipAddress);

                // 1. Check rate limiting (prevent abuse)
                if (!await CheckRateLimitAsync(ipAddress, context))
                {
                    _logger.LogWarning(
                        "⚠️ Rate limit exceeded for IP: {IP}",
                        ipAddress);
                    context.Response.StatusCode = 429; // Too Many Requests
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Rate limit exceeded"
                    });
                    return;
                }

                // 2. IP Whitelist (optional - enable in production)
                var enableIpWhitelist = _configuration.GetValue<bool>("Webhooks:EnableIpWhitelist");
                if (enableIpWhitelist && !IsIpWhitelisted(ipAddress))
                {
                    _logger.LogWarning(
                        "⚠️ Webhook from non-whitelisted IP: {IP}",
                        ipAddress);
                    context.Response.StatusCode = 403; // Forbidden
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Access denied"
                    });
                    return;
                }

                // 3. Log request body for debugging (careful in production!)
                if (_configuration.GetValue<bool>("Webhooks:LogRequestBody"))
                {
                    context.Request.EnableBuffering();
                    var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                    context.Request.Body.Position = 0;

                    _logger.LogDebug(
                        "Webhook request body: {Body}",
                        body);
                }
            }

            await _next(context);
        }

        private async Task<bool> CheckRateLimitAsync(string ipAddress, HttpContext context)
        {
            // Get rate limit settings from config
            var maxRequestsPerMinute = _configuration.GetValue("Webhooks:RateLimit:MaxPerMinute", 60);
            var windowMinutes = 1;

            var now = DateTime.UtcNow;
            var windowStart = now.AddMinutes(-windowMinutes);

            // Get or create request queue for this IP
            var requests = _requestTracker.GetOrAdd(ipAddress, _ => new Queue<DateTime>());

            lock (requests)
            {
                // Remove old requests outside the window
                while (requests.Count > 0 && requests.Peek() < windowStart)
                {
                    requests.Dequeue();
                }

                // Check if limit exceeded
                if (requests.Count >= maxRequestsPerMinute)
                {
                    return false;
                }

                // Add current request
                requests.Enqueue(now);
            }

            // Cleanup old IPs periodically
            if (_requestTracker.Count > 1000)
            {
                await Task.Run(() => CleanupOldIps(windowStart));
            }

            return true;
        }

        private void CleanupOldIps(DateTime windowStart)
        {
            var ipsToRemove = _requestTracker
                .Where(kvp => kvp.Value.Count == 0 || kvp.Value.All(t => t < windowStart))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var ip in ipsToRemove)
            {
                _requestTracker.TryRemove(ip, out _);
            }
        }

        private bool IsIpWhitelisted(string ipAddress)
        {
            // Get whitelist from configuration
            var whitelist = _configuration
                .GetSection("Webhooks:IpWhitelist")
                .Get<string[]>() ?? Array.Empty<string>();

            if (whitelist.Length == 0)
                return true; // No whitelist = allow all

            // Paymob IPs (example - get real ones from Paymob docs)
            var paymobIps = new[]
            {
                "3.127.194.93",
                "35.158.182.206",
                // Add actual Paymob IPs
            };

            // PayPal IPs (example - get real ones from PayPal docs)
            var paypalIps = new[]
            {
                "173.0.82.126",
                "173.0.82.254",
                // Add actual PayPal IPs
            };

            return whitelist.Contains(ipAddress) ||
                   paymobIps.Contains(ipAddress) ||
                   paypalIps.Contains(ipAddress);
        }
    }

    // Extension method for easy registration
    /// <summary>
    /// Provides extension methods for registering the WebhookSecurityMiddleware in an ASP.NET Core application's
    /// request pipeline.
    /// </summary>
    public static class WebhookSecurityMiddlewareExtensions
    {
        /// <summary>
        /// Adds middleware to the application's request pipeline to validate and secure incoming webhook requests.
        /// </summary>
        /// <remarks>This extension method should be called early in the middleware pipeline to ensure
        /// that webhook requests are validated before further processing. It is typically used when configuring web
        /// applications that receive webhook callbacks from external services.</remarks>
        /// <param name="builder">The application builder to configure the middleware pipeline.</param>
        /// <returns>The original <see cref="IApplicationBuilder"/> instance with the webhook security middleware configured.</returns>
        public static IApplicationBuilder UseWebhookSecurity(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebhookSecurityMiddleware>();
        }
    }
}