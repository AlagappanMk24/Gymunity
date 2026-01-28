using Gymunity.Admin.MVC.ViewModels.Notifications;
using Gymunity.Application.Contracts.Services.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gymunity.Admin.MVC.Controllers
{
    [Authorize]
    [Route("admin/notifications")]
    public class NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger) : BaseAdminController
    {
        private readonly INotificationService _notificationService = notificationService;
        private readonly ILogger<NotificationsController> _logger = logger;

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var notifications = await _notificationService.GetUserNotificationsAsync(userId);
                var unreadCount = await _notificationService.GetUnreadNotificationCountAsync(userId);

                var viewModel = new NotificationsListViewModel
                {
                    Notifications = notifications.Select(n => new NotificationViewModel
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Message = n.Message,
                        Type = n.Type.ToString(),
                        RelatedEntityId = n.RelatedEntityId,
                        CreatedAt = n.CreatedAt,
                        IsRead = n.IsRead
                    }),
                    UnreadCount = unreadCount,
                    TotalCount = notifications.Count()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notifications");
                return View(new NotificationsListViewModel());
            }
        }

        [HttpPost("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                await _notificationService.MarkNotificationAsReadAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                await _notificationService.MarkAllNotificationsAsReadAsync(userId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkNotificationRead([FromBody] MarkNotificationReadRequest request)
        {
            try
            {
                if (request?.NotificationId <= 0)
                    return BadRequest(new { success = false, message = "Invalid notification ID" });

                await _notificationService.MarkNotificationAsReadAsync(request.NotificationId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                await _notificationService.MarkAllNotificationsAsReadAsync(userId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
                return Json(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return Json(new { count = 0 });
            }
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly: true);
                var viewModels = notifications.Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type.ToString(),
                    RelatedEntityId = n.RelatedEntityId,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead
                });

                return PartialView("_NotificationsPartial", viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notifications");
                return BadRequest(ex.Message);
            }
        }
    }
}