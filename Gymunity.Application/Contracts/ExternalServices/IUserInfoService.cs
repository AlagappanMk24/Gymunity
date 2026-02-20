namespace Gymunity.Application.Contracts.ExternalServices
{
    public interface IUserInfoService
    {
        /// <summary>
        /// Gets client IP address from HttpContext
        /// </summary>
        string GetClientIpAddress();

        /// <summary>
        /// Gets user agent (device/browser info) from HttpContext
        /// </summary>
        string GetUserAgent();

        /// <summary>
        /// Gets location from IP address (requires external service)
        /// </summary>
        Task<string> GetLocationFromIpAsync(string ipAddress);

        /// <summary>
        /// Gets device/browser friendly name from user agent
        /// </summary>
        string GetDeviceFromUserAgent(string userAgent);

        /// <summary>
        /// Gets all client info in one call
        /// </summary>
        Task<ClientInfo> GetClientInfoAsync();
    }
    public class ClientInfo
    {
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Location { get; set; }
        public string Device { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}