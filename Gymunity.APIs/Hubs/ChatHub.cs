using Gymunity.Application.Contracts.ExternalServices.RealTime;
using Gymunity.Application.Contracts.Services.Communication;
using Gymunity.Application.DTOs.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Gymunity.APIs.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time messaging
    /// Handles message sending, receiving, and read notifications
    /// </summary>
    [Authorize]
    public class ChatHub(
        IChatService chatService,
        INotificationService notificationService,
        ISignalRConnectionManager connectionManager,
        ILogger<ChatHub> logger) : Hub
    {
        private readonly IChatService _chatService = chatService;
        private readonly INotificationService _notificationService = notificationService;
        private readonly ISignalRConnectionManager _connectionManager = connectionManager;
        private readonly ILogger<ChatHub> _logger = logger;

        /// <summary>
        /// Called when a user connects to the hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _connectionManager.AddConnection(userId, Context.ConnectionId);
                _logger.LogInformation($"User {userId} connected with connection ID {Context.ConnectionId}");
                
                // Notify all connected clients that user is online
                await Clients.All.SendAsync("UserOnline", userId);
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a user disconnects from the hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _connectionManager.RemoveConnection(Context.ConnectionId);
            
            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation($"User {userId} disconnected");
                
                // Notify all connected clients that user is offline if no more connections
                var remainingConnections = _connectionManager.GetAllConnections(userId);
                if (!remainingConnections.Any())
                {
                    await Clients.All.SendAsync("UserOffline", userId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Send a message to a specific thread
        /// </summary>
        public async Task SendMessage(int threadId, SendMessageRequest request)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                var message = await _chatService.SendMessageAsync(threadId, userId, request);

                // Notify all clients in the message thread group
                await Clients.Group($"thread-{threadId}").SendAsync("MessageReceived", message);

                _logger.LogInformation($"Message sent to thread {threadId} by user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        /// <summary>
        /// Join a message thread group for real-time updates
        /// </summary>
        public async Task JoinThread(int threadId)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("Error", "User not authenticated");
                    return;
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, $"thread-{threadId}");
                await Clients.Group($"thread-{threadId}").SendAsync("UserJoinedThread", userId);

                _logger.LogInformation($"User {userId} joined thread {threadId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error joining thread: {ex.Message}");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        /// <summary>
        /// Leave a message thread group
        /// </summary>
        public async Task LeaveThread(int threadId)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"thread-{threadId}");
                
                if (!string.IsNullOrEmpty(userId))
                {
                    await Clients.Group($"thread-{threadId}").SendAsync("UserLeftThread", userId);
                    _logger.LogInformation($"User {userId} left thread {threadId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error leaving thread: {ex.Message}");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        /// <summary>
        /// Mark a message as read
        /// </summary>
        public async Task MarkMessageAsRead(long messageId, int threadId)
        {
            try
            {
                await _chatService.MarkMessageAsReadAsync(messageId);
                await Clients.Group($"thread-{threadId}").SendAsync("MessageMarkedAsRead", messageId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking message as read: {ex.Message}");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        /// <summary>
        /// Mark entire thread as read
        /// </summary>
        public async Task MarkThreadAsRead(int threadId)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await _chatService.MarkThreadAsReadAsync(threadId, userId);
                    await Clients.Group($"thread-{threadId}").SendAsync("ThreadMarkedAsRead", threadId, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking thread as read: {ex.Message}");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        /// <summary>
        /// Typing indicator - notify others that user is typing
        /// </summary>
        public async Task UserTyping(int threadId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Clients.Group($"thread-{threadId}").SendAsync("UserTyping", userId);
            }
        }

        /// <summary>
        /// User stopped typing
        /// </summary>
        public async Task UserStoppedTyping(int threadId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Clients.Group($"thread-{threadId}").SendAsync("UserStoppedTyping", userId);
            }
        }
    }
}
