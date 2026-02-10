using Gymunity.Admin.MVC.Services.Interfaces;
using Gymunity.Application.Contracts.Services.Admin;
using Gymunity.Application.Services.Admin;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Gymunity.Admin.MVC.Services
{
    /// <summary>
    /// Handles user management-related notifications
    /// Subscribes to events from UserManagementService and sends notifications to admins
    /// </summary>
    public class UserNotificationService : IUserNotificationService
    {
        private readonly IAdminNotificationService _notificationService;
        private readonly IUserManagementService _userManagementService;
        private readonly IAdminUserResolverService _adminUserResolver;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<UserNotificationService> _logger;

        public UserNotificationService(
            IAdminNotificationService notificationService,
            IUserManagementService userManagementService,
            IAdminUserResolverService adminUserResolver,
            UserManager<AppUser> userManager,
            ILogger<UserNotificationService> logger)
        {
            _notificationService = notificationService;
            _userManagementService = userManagementService;
            _adminUserResolver = adminUserResolver;
            _userManager = userManager;
            _logger = logger;

            // ✅ Subscribe to events from UserManagementService
            _userManagementService.UserSuspendedAsync += OnUserSuspendedAsync;
            _userManagementService.UserReactivatedAsync += OnUserReactivatedAsync;
            _userManagementService.UserDeletedAsync += OnUserDeletedAsync;
            _userManagementService.UserRoleChangedAsync += OnUserRoleChangedAsync;
        }

        /// <summary>
        /// Handle user suspension event
        /// Creates notification to admins about user suspension
        /// </summary>
        private async Task OnUserSuspendedAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for suspension notification", userId);
                    return;
                }

                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about user suspension {UserId}", userId);
                    return;
                }

                await _notificationService.NotifyAccountSuspendedAsync(
                    admin.Id,
                    user.FullName,
                    user.Role.ToString(),
                    user.Id);

                _logger.LogInformation("Admin notified of user suspension {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send user suspension notification for user {UserId}", userId);
                // Non-blocking - don't rethrow
            }
        }

        /// <summary>
        /// Handle user reactivation event
        /// Creates notification to admins about user reactivation
        /// </summary>
        private async Task OnUserReactivatedAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for reactivation notification", userId);
                    return;
                }

                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about user reactivation {UserId}", userId);
                    return;
                }

                await _notificationService.NotifyAccountReactivatedAsync(
                    admin.Id,
                    user.FullName,
                    user.Role.ToString(),
                    user.Id);

                _logger.LogInformation("Admin notified of user reactivation {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send user reactivation notification for user {UserId}", userId);
                // Non-blocking - don't rethrow
            }
        }

        /// <summary>
        /// Handle user deletion event
        /// Creates notification to admins about user deletion
        /// </summary>
        private async Task OnUserDeletedAsync(string userId)
        {
            try
            {
                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about user deletion {UserId}", userId);
                    return;
                }

                await _notificationService.NotifyContentDeletedAsync(
                    admin.Id,
                    "User",
                    userId,
                    userId);

                _logger.LogInformation("Admin notified of user deletion {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send user deletion notification for user {UserId}", userId);
                // Non-blocking - don't rethrow
            }
        }

        /// <summary>
        /// Handle user role change event
        /// Creates notification to admins about user role change
        /// </summary>
        private async Task OnUserRoleChangedAsync(string userId, UserRole newRole)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for role change notification", userId);
                    return;
                }

                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about role change {UserId}", userId);
                    return;
                }

                await _notificationService.CreateAdminNotificationAsync(
                    admin.Id,
                    "User Role Changed",
                    $"User {user.FullName} role changed to {newRole}",
                    NotificationType.SystemNotification,
                    user.Id);

                _logger.LogInformation("Admin notified of user role change {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send role change notification for user {UserId}", userId);
                // Non-blocking - don't rethrow
            }
        }
    }
}
