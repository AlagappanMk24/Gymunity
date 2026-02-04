using Gymunity.APIs.Responses;
using Gymunity.Application.Contracts.Services.Communication;
using Gymunity.Application.DTOs.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gymunity.APIs.Controllers
{
    /// <summary>
    /// Provides API endpoints for managing chat threads and messages for authenticated users.
    /// </summary>
    /// <remarks>All actions in this controller require the user to be authenticated. The controller enables
    /// users to retrieve their chat threads, view messages within a thread, send messages, and mark messages or entire
    /// threads as read. Most endpoints return results in a standard response format indicating success or failure. For
    /// real-time messaging, consider using SignalR in addition to these endpoints.</remarks>
    [Authorize]
    public class ChatController(IChatService chatService, ILogger<ChatController> logger) : BaseApiController
    {
        private readonly IChatService _chatService = chatService;
        private readonly ILogger<ChatController> _logger = logger;

        private string? GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);
        /// <summary>
        /// Get all chat threads for the current user
        /// </summary>
        [HttpGet("threads")]
        public async Task<ActionResult<ApiResponse<IEnumerable<object?>>>> GetUserChats()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponse(401));

                var chats = await _chatService.GetUserChatsAsync(userId);
                return Ok(new { success = true, data = chats });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching user chats: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get messages for a specific thread
        /// </summary>
        [HttpGet("threads/{threadId}/messages")]
        public async Task<ActionResult> GetThreadMessages(int threadId)
        {
            try
            {
                var messages = await _chatService.GetMessageThreadAsync(threadId);
                return Ok(new { success = true, data = messages });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching thread messages: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Sends a new message to the specified chat thread.
        /// </summary>
        /// <remarks>Returns an unauthorized response if the user is not authenticated. The message is
        /// sent on behalf of the currently authenticated user.</remarks>
        /// <param name="threadId">The unique identifier of the chat thread to which the message will be sent.</param>
        /// <param name="request">An object containing the details of the message to send, including content and any relevant metadata. Cannot
        /// be null.</param>
        /// <returns>An ActionResult containing an ApiResponse with the details of the sent message if successful; otherwise, an
        /// error response.</returns>
        [HttpPost("threads/{threadId}/messages")]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> SendMessage(int threadId, [FromBody] SendMessageRequest request)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var message = await _chatService.SendMessageAsync(threadId, userId, request);
                return Ok(new ApiResponse<MessageResponse>(message));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Mark a message as read
        /// </summary>
        [HttpPut("messages/{messageId}/read")]
        public async Task<ActionResult> MarkMessageAsRead(long messageId)
        {
            try
            {
                var result = await _chatService.MarkMessageAsReadAsync(messageId);
                if (!result)
                    return NotFound(new { success = false, message = "Message not found" });

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking message as read: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Mark entire thread as read
        /// </summary>
        [HttpPut("threads/{threadId}/read")]
        public async Task<ActionResult> MarkThreadAsRead(int threadId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _chatService.MarkThreadAsReadAsync(threadId, userId);
                if (!result)
                    return NotFound(new { success = false, message = "Thread not found" });

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking thread as read: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Create a new chat thread between the current user and another user
        /// </summary>
        [HttpPost("threads")]
        public async Task<ActionResult<ApiResponse<CreateChatThreadResponse>>> CreateChatThread([FromBody] CreateChatThreadRequest request)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var thread = await _chatService.CreateChatThreadAsync(userId, request.OtherUserId);
                return Ok(new ApiResponse<CreateChatThreadResponse>(thread));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Attempted to create duplicate chat thread: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"Invalid argument creating chat thread: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating chat thread: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a chat thread
        /// </summary>
        [HttpDelete("threads/{threadId}")]
        public async Task<ActionResult> DeleteThread(int threadId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _chatService.DeleteThreadAsync(threadId);
                if (!result)
                    return NotFound(new { success = false, message = "Thread not found" });

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting chat thread: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
