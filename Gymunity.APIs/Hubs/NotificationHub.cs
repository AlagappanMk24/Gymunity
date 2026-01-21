using Gymunity.Application.Contracts.ExternalServices.RealTime;
using Gymunity.Application.Contracts.Services.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ITI.Gymunity.FP.APIs.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time notifications
    /// Handles notification delivery and management
    /// </summary>
    [Authorize]
    public class NotificationHub(
        INotificationService notificationService,
        ISignalRConnectionManager connectionManager,
        ILogger<NotificationHub> logger) : Hub
    {
        private readonly INotificationService _notificationService = notificationService;
        private readonly ISignalRConnectionManager _connectionManager = connectionManager;
        private readonly ILogger<NotificationHub> _logger = logger;

        /// <summary>
        /// Called when a user connects to the notification hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _connectionManager.AddConnection(userId, Context.ConnectionId);
                _logger.LogInformation($"User {userId} connected to notification hub with connection ID {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a user disconnects from the notification hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _connectionManager.RemoveConnection(Context.ConnectionId);
            
            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation($"User {userId} disconnected from notification hub");
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Get all unread notifications for the current user
        /// </summary>
        public async Task GetUnreadNotifications()
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly: true);
                await Clients.Caller.SendAsync("UnreadNotifications", notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching unread notifications: {ex.Message}");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        /// <summary>
        /// Get unread notification count
        /// </summary>
        public async Task GetUnreadNotificationCount()
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
                await Clients.Caller.SendAsync("UnreadNotificationCount", count);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching unread notification count: {ex.Message}");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        public async Task MarkNotificationAsRead(int notificationId)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                await _notificationService.MarkNotificationAsReadAsync(notificationId);
                await Clients.User(userId).SendAsync("NotificationMarkedAsRead", notificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking notification as read: {ex.Message}");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        public async Task MarkAllNotificationsAsRead()
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                await _notificationService.MarkAllNotificationsAsReadAsync(userId);
                await Clients.User(userId).SendAsync("AllNotificationsMarkedAsRead");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking all notifications as read: {ex.Message}");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }
    }
}
