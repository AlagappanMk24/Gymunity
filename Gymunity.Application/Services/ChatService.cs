using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Gymunity.Domain;
using Gymunity.Application.Contracts.ExternalServices;
using Gymunity.Application.Contracts.Services.Communication;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Application.DTOs.Messaging;
using Gymunity.Domain.Entities.Messaging;
using Gymunity.Application.Specifications.Chat;

namespace Gymunity.Application.Services
{
    public class ChatService(IUnitOfWork unitOfWork, 
            UserManager<AppUser> userManager, 
            IMapper mapper,
            ILogger<ChatService> logger,
            IImageUrlResolver imageUrlResolver) : IChatService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<ChatService> _logger = logger;
        private readonly IImageUrlResolver _imageUrlResolver = imageUrlResolver;

        public async Task<CreateChatThreadResponse> CreateChatThreadAsync(string clientId, string trainerId)
        {
            // Validate both users exist
            var client = await _userManager.FindByIdAsync(clientId);
            var trainer = await _userManager.FindByIdAsync(trainerId);

            if (client == null)
                throw new ArgumentException("Client user not found");
            if (trainer == null)
                throw new ArgumentException("Trainer user not found");

            // Check if thread already exists (both directions)
            var threads = await _unitOfWork.Repository<MessageThread>().GetAllAsync();
            var existingThread = threads.FirstOrDefault(t => 
                t.ClientId == clientId && t.TrainerId == trainerId ||
                t.ClientId == trainerId && t.TrainerId == clientId);

            if (existingThread != null)
                throw new InvalidOperationException("A chat thread already exists between these users");

            // Create new thread
            var newThread = new MessageThread
            {
                ClientId = clientId,
                TrainerId = trainerId,
                LastMessageAt = DateTime.UtcNow,
                IsPriority = false
            };

            _unitOfWork.Repository<MessageThread>().Add(newThread);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                _logger.LogError("Failed to create message thread between client {ClientId} and trainer {TrainerId}", 
                    clientId, trainerId);
                throw;
            }

            return new CreateChatThreadResponse
            {
                Id = newThread.Id,
                ClientId = newThread.ClientId,
                TrainerId = newThread.TrainerId,
                CreatedAt = newThread.CreatedAt.DateTime,
                LastMessageAt = newThread.LastMessageAt
            };
        }

        public async Task<MessageResponse> SendMessageAsync(int threadId, string senderId, SendMessageRequest request)
        {
            var threadSpecs = new MessageThreadWithClientTrainerAndMessagesSpecs(threadId);
            var thread = await _unitOfWork.Repository<MessageThread>().GetWithSpecsAsync(threadSpecs)
                    ?? throw new ArgumentException("Message thread not found");
            var message = new Message
            {
                ThreadId = threadId,
                SenderId = senderId,
                Content = request.Content,
                MediaUrl = request.MediaUrl,
                Type = request.Type,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            thread.Messages.Add(message);

            // Update thread's last message time
            thread.LastMessageAt = DateTime.UtcNow;
            _unitOfWork.Repository<MessageThread>().Update(thread);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                _logger.LogError("Failed to send message in thread {ThreadId} by user {SenderId}", threadId, senderId);
                throw;
            }

            var sender = await _userManager.FindByIdAsync(senderId);

            var response = _mapper.Map<MessageResponse>(message);
            response.SenderName = sender?.FullName ?? "Unknown";

            return response;
        }

        public async Task<IEnumerable<MessageResponse>> GetMessageThreadAsync(int threadId)
        {
            var messages = await _unitOfWork.Repository<Message>().GetAllAsync();
            var threadMessages = messages.Where(m => m.ThreadId == threadId).ToList();

            var result = new List<MessageResponse>();
            foreach (var message in threadMessages.OrderBy(m => m.CreatedAt))
            {
                var sender = await _userManager.FindByIdAsync(message.SenderId);

                result.Add(new MessageResponse
                {
                    Id = message.Id,
                    ThreadId = message.ThreadId,
                    SenderId = message.SenderId,
                    SenderName = sender?.FullName ?? "Unknown",
                    SenderProfilePhoto = _imageUrlResolver.ResolveImageUrl(sender?.ProfilePhotoUrl ?? "")!,
                    Content = message.Content,
                    MediaUrl = message.MediaUrl,
                    Type = message.Type,
                    CreatedAt = message.CreatedAt,
                    IsRead = message.IsRead
                });
            }

            return result;
        }

        public async Task<IEnumerable<object>> GetUserChatsAsync(string userId)
        {
            var threads = await _unitOfWork.Repository<MessageThread>().GetAllAsync();
            var userThreads = threads
                .Where(t => t.ClientId == userId || t.TrainerId == userId)
                .ToList();

            var result = new List<dynamic>();
            foreach (var thread in userThreads.OrderByDescending(t => t.LastMessageAt))
            {
                var otherUserId = thread.ClientId == userId ? thread.TrainerId : thread.ClientId;
                var otherUser = await _userManager.FindByIdAsync(otherUserId);

                var allMessages = await _unitOfWork.Repository<Message>().GetAllAsync();
                var threadMessages = allMessages.Where(m => m.ThreadId == thread.Id).ToList();
                var unreadCount = threadMessages
                    .Where(m => m.SenderId != userId && !m.IsRead)
                    .Count();

                result.Add(new
                {
                    thread.Id,
                    thread.ClientId,
                    thread.TrainerId,
                    thread.LastMessageAt,
                    thread.IsPriority,
                    OtherUserId = otherUserId,
                    OtherUserName = otherUser?.FullName,
                    OtherUserProfilePhoto = _imageUrlResolver.ResolveImageUrl(otherUser?.ProfilePhotoUrl!),
                    UnreadCount = unreadCount
                });
            }

            return result;
        }

        public async Task<bool> MarkMessageAsReadAsync(long messageId)
        {
            var message = await _unitOfWork.Repository<Message>().GetByIdAsync((int)messageId);
            if (message == null)
                return false;

            message.IsRead = true;
            _unitOfWork.Repository<Message>().Update(message);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> MarkThreadAsReadAsync(int threadId, string userId)
        {
            var allMessages = await _unitOfWork.Repository<Message>().GetAllAsync();
            var messages = allMessages
                .Where(m => m.ThreadId == threadId && m.SenderId != userId && !m.IsRead)
                .ToList();

            foreach (var message in messages)
            {
                message.IsRead = true;
                _unitOfWork.Repository<Message>().Update(message);
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteThreadAsync(int threadId)
        {
            var thread = await _unitOfWork.Repository<MessageThread>().GetByIdAsync(threadId);
            if (thread == null)
                return false;

            var allMessages = await _unitOfWork.Repository<Message>().GetAllAsync();
            var messages = allMessages.Where(m => m.ThreadId == threadId).ToList();

            foreach (var message in messages)
            {
                _unitOfWork.Repository<Message>().Delete(message);
            }

            _unitOfWork.Repository<MessageThread>().Delete(thread);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                _logger.LogError("Failed to delete message thread {ThreadId}", threadId);
                throw;
            }

            return true;
        }
    }
}
