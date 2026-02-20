using Gymunity.Application.Contracts.ExternalServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Gymunity.Infrastructure.ExternalServices
{
    public class UserInfoService(
    IHttpContextAccessor httpContextAccessor,
    ILogger<UserInfoService> logger,
    IHttpClientFactory httpClientFactory) : IUserInfoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<UserInfoService> _logger = logger;
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

        // Common user agent patterns for device detection
        private readonly Dictionary<string, string> _devicePatterns = new()
        {
            { "Windows", "Windows PC" },
            { "Macintosh", "Mac" },
            { "iPhone", "iPhone" },
            { "iPad", "iPad" },
            { "Android", "Android Device" },
            { "Linux", "Linux PC" },
            { "CrOS", "Chromebook" }
        };

        // Browser detection
        private readonly Dictionary<string, string> _browserPatterns = new()
        {
            { "Chrome", "Chrome" },
            { "Firefox", "Firefox" },
            { "Safari", "Safari" },
            { "Edge", "Edge" },
            { "Opera", "Opera" },
            { "MSIE", "Internet Explorer" },
            { "Trident", "Internet Explorer" }
        };
        public string GetClientIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "Unknown";

            // Log all headers first
            _logger.LogInformation("=== ALL HEADERS ===");
            foreach (var header in context.Request.Headers)
            {
                _logger.LogInformation("Header: {Key} = {Value}", header.Key, header.Value);
            }

            string realIp;
            // Try X-Forwarded-For header first (ngrok adds this)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                _logger.LogInformation("✅ Found X-Forwarded-For header: {Header}", forwardedFor);
                // Get the first IP in the list (client's real IP)
                 realIp = forwardedFor.Split(',').First().Trim();
                _logger.LogInformation("✅ Real client IP from X-Forwarded-For: {Ip}", realIp);
                return realIp;
            }

            // Try X-Real-IP header (ngrok might use this)
             realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                _logger.LogInformation("✅ Found X-Real-IP header: {Header}", realIp);
                return realIp;
            }

            // Try X-Forwarded header
            var forwarded = context.Request.Headers["X-Forwarded"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
            {
                _logger.LogInformation("✅ Found X-Forwarded header: {Header}", forwarded);
                return forwarded.Split(',').First().Trim();
            }

            // Try CF-Connecting-IP (Cloudflare)
            var cfIp = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(cfIp))
            {
                _logger.LogInformation("✅ Found CF-Connecting-IP header: {Header}", cfIp);
                return cfIp;
            }

            // Fallback to remote IP address
            var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            _logger.LogInformation("❌ No proxy headers found, using remote IP: {RemoteIp}", remoteIp);
            return remoteIp;
        }
        public string GetUserAgent()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "Unknown";

            return context.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
        }

        public async Task<string> GetLocationFromIpAsync(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "Unknown" || ipAddress == "::1" || ipAddress == "127.0.0.1")
            {
                return "Localhost";
            }

            try
            {
                // Using ip-api.com (free, no API key required for limited usage)
                var response = await _httpClient.GetAsync($"http://ip-api.com/json/{ipAddress}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    var status = root.GetProperty("status").GetString();
                    if (status == "success")
                    {
                        var city = root.GetProperty("city").GetString();
                        var region = root.GetProperty("regionName").GetString();
                        var country = root.GetProperty("country").GetString();

                        return $"{city}, {region}, {country}";
                    }
                }

                // Alternative: ipinfo.io (requires token)
                // var token = "YOUR_TOKEN";
                // response = await _httpClient.GetAsync($"https://ipinfo.io/{ipAddress}/json?token={token}");

                _logger.LogWarning("Could not get location for IP: {IpAddress}", ipAddress);
                return "Unknown Location";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting location for IP: {IpAddress}", ipAddress);
                return "Unknown Location";
            }
        }
        public string GetDeviceFromUserAgent(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent) || userAgent == "Unknown")
                return "Unknown Device";

            // Detect OS/Device
            string device = "Unknown Device";
            foreach (var pattern in _devicePatterns)
            {
                if (userAgent.Contains(pattern.Key))
                {
                    device = pattern.Value;
                    break;
                }
            }

            // Detect Browser
            string browser = "Unknown Browser";
            foreach (var pattern in _browserPatterns)
            {
                if (userAgent.Contains(pattern.Key))
                {
                    browser = pattern.Value;
                    break;
                }
            }

            return $"{device} - {browser}";
        }

        public async Task<ClientInfo> GetClientInfoAsync()
        {
            var ipAddress = GetClientIpAddress();
            _logger.LogInformation("=== IP DETECTION ===");
            _logger.LogInformation("Raw IP Address: {IpAddress}", ipAddress);

            // Log all headers for debugging
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                foreach (var header in context.Request.Headers)
                {
                    _logger.LogInformation("Header: {Key} = {Value}", header.Key, header.Value);
                }
            }

            var userAgent = GetUserAgent();
            var device = GetDeviceFromUserAgent(userAgent);
            var location = await GetLocationFromIpAsync(ipAddress);

            _logger.LogInformation("Final Location: {Location}", location);
            _logger.LogInformation("====================");

            return new ClientInfo
            {
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Location = location,
                Device = device,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}