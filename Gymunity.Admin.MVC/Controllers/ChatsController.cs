using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Newtonsoft.Json.Linq;
using Gymunity.Application.Contracts.Services.Communication;
using Gymunity.Admin.MVC.ViewModels.Chat;
using Gymunity.Application.DTOs.Messaging;

namespace Gymunity.Admin.MVC.Controllers
{
    [Authorize]
    [Route("admin/chats")]
    public class ChatsController(
        IChatService chatService,
        ILogger<ChatsController> logger) : BaseAdminController
    {
        private readonly IChatService _chatService = chatService;
        private readonly ILogger<ChatsController> _logger = logger;

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var chatData = await _chatService.GetUserChatsAsync(userId);

                var threads = new List<ChatThreadViewModel>();
                foreach (var chat in chatData)
                {
                    var chatObj = JObject.FromObject(chat);
                    threads.Add(new ChatThreadViewModel
                    {
                        Id = chatObj["id"]?.Value<int>() ?? 0,
                        ClientId = chatObj["clientId"]?.Value<string>() ?? "",
                        ClientName = chatObj["otherUserName"]?.Value<string>(),
                        ClientProfilePhoto = chatObj["otherUserProfilePhoto"]?.Value<string>(),
                        TrainerId = chatObj["trainerId"]?.Value<string>() ?? "",
                        LastMessageAt = chatObj["lastMessageAt"]?.Value<DateTime?>(),
                        IsPriority = chatObj["isPriority"]?.Value<bool>() ?? false,
                        UnreadCount = chatObj["unreadCount"]?.Value<int>() ?? 0
                    });
                }

                var viewModel = new ChatListViewModel
                {
                    Threads = threads.OrderByDescending(t => t.LastMessageAt),
                    TotalUnreadCount = threads.Sum(t => t.UnreadCount)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chats");
                return View(new ChatListViewModel());
            }
        }

        [HttpGet("thread/{threadId}")]
        public async Task<IActionResult> GetThread(int threadId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var messages = await _chatService.GetMessageThreadAsync(threadId);

                var viewModel = new ChatThreadViewModel
                {
                    Id = threadId,
                    Messages = messages.Select(m => new ChatMessageViewModel
                    {
                        Id = m.Id,
                        ThreadId = m.ThreadId,
                        SenderId = m.SenderId,
                        SenderName = m.SenderName,
                        SenderProfilePhoto = m.SenderProfilePhoto,
                        Content = m.Content,
                        MediaUrl = m.MediaUrl,
                        Type = (int)m.Type,
                        CreatedAt = m.CreatedAt,
                        IsRead = m.IsRead
                    })
                };

                return PartialView("_ChatThreadPartial", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chat thread");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("send-message")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int threadId, [FromBody] SendMessageRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var message = await _chatService.SendMessageAsync(threadId, userId, request);

                var viewModel = new ChatMessageViewModel
                {
                    Id = message.Id,
                    ThreadId = message.ThreadId,
                    SenderId = message.SenderId,
                    SenderName = message.SenderName,
                    SenderProfilePhoto = message.SenderProfilePhoto,
                    Content = message.Content,
                    MediaUrl = message.MediaUrl,
                    Type = (int)message.Type,
                    CreatedAt = message.CreatedAt,
                    IsRead = message.IsRead
                };

                return Json(new { success = true, message = viewModel });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("mark-as-read/{messageId}")]
        public async Task<IActionResult> MarkMessageAsRead(long messageId)
        {
            try
            {
                var result = await _chatService.MarkMessageAsReadAsync(messageId);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message as read");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("mark-thread-as-read/{threadId}")]
        public async Task<IActionResult> MarkThreadAsRead(int threadId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _chatService.MarkThreadAsReadAsync(threadId, userId);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking thread as read");
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

                var chatData = await _chatService.GetUserChatsAsync(userId);
                int totalUnread = 0;
                foreach (var chat in chatData)
                {
                    var chatObj = JObject.FromObject(chat);
                    totalUnread += chatObj["unreadCount"]?.Value<int>() ?? 0;
                }

                return Json(new { count = totalUnread });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return Json(new { count = 0 });
            }
        }
    }
}
