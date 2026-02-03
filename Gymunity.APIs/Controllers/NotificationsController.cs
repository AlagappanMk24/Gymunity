using Gymunity.APIs.Responses;
using Gymunity.Application.Contracts.Services.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gymunity.APIs.Controllers
{
    /// <summary>
    /// API controller that manages user notifications, including retrieving, counting, and updating notification status
    /// for the authenticated user.
    /// </summary>
    /// <remarks>All endpoints require the user to be authenticated. This controller provides actions to
    /// retrieve notifications, get the count of unread notifications, and mark notifications as read. It is intended to
    /// be used by client applications to manage notification state for the current user.</remarks>
    [Authorize]
    public class NotificationsController(INotificationService notificationService, 
        ILogger<NotificationsController> logger) : BaseApiController
    {
        private readonly INotificationService _notificationService = notificationService;
        private readonly ILogger<NotificationsController> _logger = logger;

        /// <summary>
        /// Get all notifications for the current user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetNotifications([FromQuery] bool unreadOnly = false)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized));

                var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);
                return Ok(new { success = true, data = notifications });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching notifications: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get unread notification count
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<ActionResult> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
                return Ok(new { success = true, data = count });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching unread count: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        [HttpPut("{notificationId}/read")]
        public async Task<ActionResult> MarkAsRead(int notificationId)
        {
            try
            {
                var result = await _notificationService.MarkNotificationAsReadAsync(notificationId);
                if (!result)
                    return NotFound(new { success = false, message = "Notification not found" });

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking notification as read: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPut("read-all")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _notificationService.MarkAllNotificationsAsReadAsync(userId);
                if (!result)
                    return BadRequest(new { success = false, message = "Failed to mark all notifications as read" });

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking all notifications as read: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
