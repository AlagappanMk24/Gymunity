using Gymunity.Application.Contracts.Services.Communication;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Gymunity.APIs.Hubs
{
    /// <summary>
    /// SignalR hub for real-time admin notifications
    /// Handles notification broadcasting to connected admin users
    /// </summary>
    public class AdminNotificationHub(ILogger<AdminNotificationHub> logger) : Hub
    {
        private readonly ILogger<AdminNotificationHub> _logger = logger;

        /// <inheritdoc/>
        //public override async Task OnConnectedAsync()
        //{
        //    var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

        //    if (!string.IsNullOrEmpty(userId))
        //    {
        //        await Groups.AddToGroupAsync(Context.ConnectionId, $"admin_{userId}");

        //        _logger.LogInformation("✅ Admin {UserName} added to admin_{UserId} and all_admins groups", userName, userId);
        //    }
        //    else
        //    {
        //        _logger.LogWarning("Connection attempt without valid user context. ConnectionId: {ConnectionId}", Context.ConnectionId);
        //    }

        //    await base.OnConnectedAsync();
        //}

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

            if (!string.IsNullOrEmpty(userId))
            {
                // Add to personal group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"admin_{userId}");

                // Add to ALL ADMINS group for broadcasts
                await Groups.AddToGroupAsync(Context.ConnectionId, "all_admins");

                _logger.LogInformation("✅ Admin {UserName} (ID: {UserId}) connected to API hub", userName, userId);

                // Send initial unread count
                try
                {
                    var notificationService = Context.GetHttpContext()?.RequestServices
                        .GetRequiredService<INotificationService>();

                    if (notificationService != null)
                    {
                        var unreadCount = await notificationService.GetUnreadNotificationCountAsync(userId);
                        await Clients.Caller.SendAsync("UpdateNotificationCount", unreadCount);
                        _logger.LogInformation("📊 Sent initial unread count {Count} to user {UserId}", unreadCount, userId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending initial unread count");
                }
            }
            else
            {
                _logger.LogWarning("❌ Connection attempt without valid user context. ConnectionId: {ConnectionId}",
                    Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        /// <inheritdoc/>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation("Admin user {UserName} disconnected from notifications. ConnectionId: {ConnectionId}", userName, Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Send notification to specific admin user
        /// </summary>
        public async Task SendNotificationToUser(string userId, string title, string message, string type, string? relatedEntityId = null)
        {
            await Clients.Group($"admin_{userId}").SendAsync("ReceiveNotification", new
            {
                Title = title,
                Message = message,
                Type = type,
                RelatedEntityId = relatedEntityId,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Broadcast notification to all admins
        /// </summary>
        public async Task BroadcastNotification(string title, string message, string type, string? relatedEntityId = null)
        {
            await Clients.All.SendAsync("ReceiveNotification", new
            {
                Title = title,
                Message = message,
                Type = type,
                RelatedEntityId = relatedEntityId,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Send alert for critical events
        /// </summary>
        public async Task SendCriticalAlert(string title, string message, string severity = "warning")
        {
            await Clients.All.SendAsync("ReceiveCriticalAlert", new
            {
                Title = title,
                Message = message,
                Severity = severity,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Update unread notification count for user
        /// </summary>
        public async Task UpdateNotificationCount(string userId, int unreadCount)
        {
            await Clients.Group($"admin_{userId}").SendAsync("UpdateNotificationCount", unreadCount);
        }

        /// <summary>
        /// Notify admin of action required
        /// </summary>
        public async Task NotifyActionRequired(string userId, string action, string targetEntity, string? relatedEntityId = null)
        {
            await Clients.Group($"admin_{userId}").SendAsync("ActionRequired", new
            {
                Action = action,
                TargetEntity = targetEntity,
                RelatedEntityId = relatedEntityId,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}