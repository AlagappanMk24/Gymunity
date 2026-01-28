using Gymunity.Application.DTOs.Admin;
using Gymunity.Domain.Enums;

namespace Gymunity.Application.Contracts.Admin
{
    /// <summary>
    /// Interface for managing users and their roles in the admin panel
    /// </summary>
    public interface IUserManagementService
    {
        /// <summary>
        /// Get all users with pagination
        /// </summary>
        Task<UserManagementListResponse> GetAllUsersAsync(int pageNumber = 1, int pageSize = 20);

        /// <summary>
        /// Search users by email, username, or full name
        /// </summary>
        Task<UserManagementListResponse> SearchUsersAsync(string searchTerm, int pageNumber = 1, int pageSize = 20);

        /// <summary>
        /// Get users filtered by role
        /// </summary>
        Task<UserManagementListResponse> GetUsersByRoleAsync(UserRole role, int pageNumber = 1, int pageSize = 20);

        /// <summary>
        /// Get specific user details
        /// </summary>
        Task<UserDetailResponse> GetUserDetailAsync(string userId);

        /// <summary>
        /// Update user information (admin can edit any user)
        /// </summary>
        Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request);

        /// <summary>
        /// Change user's role
        /// </summary>
        Task<bool> ChangeUserRoleAsync(string userId, UserRole newRole);

        /// <summary>
        /// Suspend user account
        /// </summary>
        Task<bool> SuspendUserAsync(string userId, string reason);

        /// <summary>
        /// Reactivate suspended user account
        /// </summary>
        Task<bool> ReactivateUserAsync(string userId);

        /// <summary>
        /// Delete user account (soft delete)
        /// </summary>
        Task<bool> DeleteUserAsync(string userId);

        /// <summary>
        /// Reset user password
        /// </summary>
        Task<bool> ResetUserPasswordAsync(string userId);

        /// <summary>
        /// Lock user account (prevent login)
        /// </summary>
        Task<bool> LockUserAccountAsync(string userId, int durationMinutes);

        /// <summary>
        /// Unlock user account
        /// </summary>
        Task<bool> UnlockUserAccountAsync(string userId);

        /// <summary>
        /// Get user activity log
        /// </summary>
        Task<IEnumerable<UserActivityLogResponse>> GetUserActivityLogsAsync(string userId, int pageSize = 50);

        /// <summary>
        /// Get statistics about users
        /// </summary>
        Task<UserStatisticsResponse> GetUserStatisticsAsync();

        /// <summary>
        /// Verify trainer account
        /// </summary>
        Task<bool> VerifyTrainerAsync(string trainerId);

        /// <summary>
        /// Reject trainer verification
        /// </summary>
        Task<bool> RejectTrainerVerificationAsync(string trainerId, string reason);

        /// <summary>
        /// Get pending trainer verifications
        /// </summary>
        Task<IEnumerable<PendingTrainerVerificationResponse>> GetPendingTrainerVerificationsAsync();

        /// <summary>
        /// Bulk update user roles
        /// </summary>
        Task<BulkOperationResponse> BulkUpdateUserRolesAsync(BulkRoleUpdateRequest request);

        /// <summary>
        /// Export users data
        /// </summary>
        Task<byte[]> ExportUsersAsync(string format = "csv");

        Task<byte[]> ExportStatisticsPdfAsync();
    }
}