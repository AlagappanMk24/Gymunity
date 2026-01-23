using AutoMapper;
using Gymunity.Application.Contracts.Services.Communication;
using Gymunity.Application.DTOs.Notifications;
using Gymunity.Domain;
using Gymunity.Domain.Entities;
using Gymunity.Domain.Enums;

namespace Gymunity.Application.Services
{
    public class NotificationService(IUnitOfWork unitOfWork, IMapper mapper) : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<NotificationResponse> CreateNotificationAsync(string userId, string title, string message, 
            int type, string? relatedEntityId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = (NotificationType)type,
                RelatedEntityId = relatedEntityId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _unitOfWork.Repository<Notification>().Add(notification);
            await _unitOfWork.CompleteAsync();

            return new NotificationResponse
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                RelatedEntityId = notification.RelatedEntityId,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead
            };
        }

        public async Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
        {
            var allNotifications = await _unitOfWork.Repository<Notification>().GetAllAsync();
            var query = allNotifications.Where(n => n.UserId == userId && (!unreadOnly || !n.IsRead));

            return query
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationResponse
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    RelatedEntityId = n.RelatedEntityId,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead
                });
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
        {
            var notification = await _unitOfWork.Repository<Notification>().GetByIdAsync(notificationId);
            if (notification == null)
                return false;

            notification.IsRead = true;
            _unitOfWork.Repository<Notification>().Update(notification);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(string userId)
        {
            var allNotifications = await _unitOfWork.Repository<Notification>().GetAllAsync();
            var notifications = allNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToList();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                _unitOfWork.Repository<Notification>().Update(notification);
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            var allNotifications = await _unitOfWork.Repository<Notification>().GetAllAsync();
            var notifications = allNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToList();

            return notifications.Count;
        }
    }
}