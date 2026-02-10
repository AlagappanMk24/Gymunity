using Gymunity.Admin.MVC.Services.Interfaces;
using Gymunity.Application.Contracts.ExternalServices.Auth;
using Gymunity.Application.Contracts.Services.Identity;
using Gymunity.Domain.Enums;

namespace Gymunity.Admin.MVC.Services
{
    /// <summary>
    /// Handles user registration notifications
    /// Subscribes to events from AccountService and sends notifications to admins
    /// </summary>
    public class AccountNotificationService : IAccountNotificationService
    {
        private readonly IAdminNotificationService _notificationService;
        private readonly IIdentityService _identityService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IAdminUserResolverService _adminUserResolver;
        private readonly ILogger<AccountNotificationService> _logger;

        public AccountNotificationService(
            IAdminNotificationService notificationService,
            IIdentityService identityService, 
            IGoogleAuthService googleAuthService, 
            IAdminUserResolverService adminUserResolver,
            ILogger<AccountNotificationService> logger)
        {
            _notificationService = notificationService;
            _identityService = identityService;
            _googleAuthService = googleAuthService;
            _adminUserResolver = adminUserResolver;
            _logger = logger;

            // ✅ Correctly subscribe to IdentityService for standard registrations
            _identityService.NewUserRegisteredAsync += OnNewUserRegisteredAsync;

            // ✅ Correctly subscribe to GoogleAuthService for Google registrations
            _googleAuthService.NewGoogleUserRegisteredAsync += OnNewGoogleUserRegisteredAsync;
        }

        /// <summary>
        /// Handle new standard user registration event
        /// Creates and broadcasts notification to all admins
        /// </summary>
        private async Task OnNewUserRegisteredAsync(string userId, string fullName, string email, UserRole role)
        {
            try
            {
                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about new registration {UserId}", userId);
                    return;
                }

                var notificationType = role switch
                {
                    UserRole.Client => NotificationType.NewClientRegistration,
                    UserRole.Trainer => NotificationType.NewTrainerRegistration,
                    _ => NotificationType.SystemNotification
                };

                await _notificationService.CreateAdminNotificationAsync(
                    admin.Id,
                    $"New {role} Registration",
                    $"{fullName} ({email}) has registered as a {role}",
                    notificationType,
                    userId,
                    broadcastToAll: true);

                _logger.LogInformation("Admin notified of new user registration {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send new user registration notification for user {UserId}", userId);
                // Non-blocking - don't rethrow
            }
        }

        /// <summary>
        /// Handle new Google user registration event
        /// Creates and broadcasts notification to all admins
        /// </summary>
        private async Task OnNewGoogleUserRegisteredAsync(string userId, string fullName, string email, UserRole role)
        {
            try
            {
                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about new Google registration {UserId}", userId);
                    return;
                }

                var notificationType = role switch
                {
                    UserRole.Client => NotificationType.NewClientRegistration,
                    UserRole.Trainer => NotificationType.NewTrainerRegistration,
                    _ => NotificationType.SystemNotification
                };

                await _notificationService.CreateAdminNotificationAsync(
                    admin.Id,
                    $"New {role} Registration (Google Auth)",
                    $"{fullName} ({email}) has registered as a {role} using Google authentication",
                    notificationType,
                    userId,
                    broadcastToAll: true);

                _logger.LogInformation("Admin notified of new Google user registration {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send new Google user registration notification for user {UserId}", userId);
                // Non-blocking - don't rethrow
            }
        }
    }
}
