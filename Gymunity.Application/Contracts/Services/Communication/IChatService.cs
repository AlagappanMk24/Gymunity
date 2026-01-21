using Gymunity.Application.DTOs.Messaging;

namespace Gymunity.Application.Contracts.Services.Communication
{
    public interface IChatService
    {
        Task<MessageResponse> SendMessageAsync(int threadId, string senderId, SendMessageRequest request);
        Task<IEnumerable<MessageResponse>> GetMessageThreadAsync(int threadId);
        Task<IEnumerable<object>> GetUserChatsAsync(string userId);
        Task<bool> MarkMessageAsReadAsync(long messageId);
        Task<bool> MarkThreadAsReadAsync(int threadId, string userId);
        Task<CreateChatThreadResponse> CreateChatThreadAsync(string clientId, string trainerId);
        Task<bool> DeleteThreadAsync(int threadId);
    }
}