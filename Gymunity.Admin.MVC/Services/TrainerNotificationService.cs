using Gymunity.Admin.MVC.Services.Interfaces;
using Gymunity.Application.Services.Admin;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Enums;

namespace Gymunity.Admin.MVC.Services
{
    /// <summary>
    /// Handles trainer-related notifications
    /// Subscribes to events from TrainerAdminService and sends notifications to admins
    /// </summary>
    public class TrainerNotificationService
    {
        private readonly IAdminNotificationService _notificationService;
        private readonly TrainerAdminService _trainerAdminService;
        private readonly AdminUserResolverService _adminUserResolver;
        private readonly ILogger<TrainerNotificationService> _logger;

        public TrainerNotificationService(
            IAdminNotificationService notificationService,
            TrainerAdminService trainerAdminService,
            AdminUserResolverService adminUserResolver,
            ILogger<TrainerNotificationService> logger)
        {
            _notificationService = notificationService;
            _trainerAdminService = trainerAdminService;
            _adminUserResolver = adminUserResolver;
            _logger = logger;

            // ✅ Subscribe to events from TrainerAdminService
            _trainerAdminService.TrainerVerifiedAsync += OnTrainerVerifiedAsync;
            _trainerAdminService.TrainerRejectedAsync += OnTrainerRejectedAsync;
            _trainerAdminService.TrainerSuspendedAsync += OnTrainerSuspendedAsync;
            _trainerAdminService.TrainerReactivatedAsync += OnTrainerReactivatedAsync;
        }

        /// <summary>
        /// Handle trainer verification event
        /// Creates notification to admins about trainer verification
        /// </summary>
        private async Task OnTrainerVerifiedAsync(int trainerId, TrainerProfile trainer)
        {
            try
            {
                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about trainer verification {TrainerId}", trainerId);
                    return;
                }

                var trainerName = trainer.User?.FullName ?? "Unknown Trainer";
                
                await _notificationService.CreateAdminNotificationAsync(
                    admin.Id,
                    "Trainer Verified",
                    $"Trainer {trainerName} ({trainer.Handle}) has been successfully verified",
                    NotificationType.TrainerApproved,
                    trainer.Id.ToString(),
                    broadcastToAll: true);

                _logger.LogInformation("Admin notified of trainer verification {TrainerId}", trainerId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send trainer verification notification for trainer {TrainerId}", trainerId);
                // Non-blocking - don't rethrow
            }
        }

        /// <summary>
        /// Handle trainer rejection event
        /// Creates notification to admins about trainer rejection
        /// </summary>
        private async Task OnTrainerRejectedAsync(int trainerId, TrainerProfile trainer)
        {
            try
            {
                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about trainer rejection {TrainerId}", trainerId);
                    return;
                }

                var trainerName = trainer.User?.FullName ?? "Unknown Trainer";
                
                await _notificationService.CreateAdminNotificationAsync(
                    admin.Id,
                    "Trainer Rejected",
                    $"Trainer {trainerName} ({trainer.Handle}) has been rejected",
                    NotificationType.TrainerRejected,
                    trainer.Id.ToString(),
                    broadcastToAll: false);

                _logger.LogInformation("Admin notified of trainer rejection {TrainerId}", trainerId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send trainer rejection notification for trainer {TrainerId}", trainerId);
                // Non-blocking - don't rethrow
            }
        }

        /// <summary>
        /// Handle trainer suspension event
        /// Creates notification to admins about trainer suspension
        /// </summary>
        private async Task OnTrainerSuspendedAsync(int trainerId, TrainerProfile trainer)
        {
            try
            {
                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about trainer suspension {TrainerId}", trainerId);
                    return;
                }

                var trainerName = trainer.User?.FullName ?? "Unknown Trainer";
                
                await _notificationService.NotifyAccountSuspendedAsync(
                    admin.Id,
                    trainerName,
                    "Trainer",
                    trainer.UserId);

                _logger.LogInformation("Admin notified of trainer suspension {TrainerId}", trainerId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send trainer suspension notification for trainer {TrainerId}", trainerId);
                // Non-blocking - don't rethrow
            }
        }

        /// <summary>
        /// Handle trainer reactivation event
        /// Creates notification to admins about trainer reactivation
        /// </summary>
        private async Task OnTrainerReactivatedAsync(int trainerId, TrainerProfile trainer)
        {
            try
            {
                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about trainer reactivation {TrainerId}", trainerId);
                    return;
                }

                var trainerName = trainer.User?.FullName ?? "Unknown Trainer";
                
                await _notificationService.NotifyAccountReactivatedAsync(
                    admin.Id,
                    trainerName,
                    "Trainer",
                    trainer.UserId);

                _logger.LogInformation("Admin notified of trainer reactivation {TrainerId}", trainerId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send trainer reactivation notification for trainer {TrainerId}", trainerId);
                // Non-blocking - don't rethrow
            }
        }
    }
}
