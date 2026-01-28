using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gymunity.Application.Contracts.Admin;
using Gymunity.Domain.Enums;
using Gymunity.Application.DTOs.Admin;
using UserDetailResponse = Gymunity.Admin.MVC.ViewModels.Users.UserDetailResponse;
using AppUserDetailResponse = Gymunity.Application.DTOs.Admin.UserDetailResponse;
using Gymunity.Admin.MVC.ViewModels.Users;

namespace Gymunity.Admin.MVC.Controllers
{
    /// <summary>
    /// Controller for managing users and roles in the admin panel
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("admin/users")]
    public class UsersController(IUserManagementService userManagementService, ILogger<UsersController> logger) : BaseAdminController
    {
        private readonly IUserManagementService _userManagementService = userManagementService;
        private readonly ILogger<UsersController> _logger = logger;

        /// <summary>
        /// Display list of all users with pagination
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                SetPageTitle("Manage Users");
                SetBreadcrumbs("Users");

                var result = await _userManagementService.GetAllUsersAsync(pageNumber, pageSize);
                var viewModel = new UsersListViewModel
                {
                    Users = result.Users.ToList(),
                    TotalCount = result.TotalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = result.TotalPages
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users list");
                ShowErrorMessage("Error loading users list");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Search users by email, username, or name
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search(string searchTerm = "", int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                SetPageTitle("Search Users");
                SetBreadcrumbs("Users", "Search");

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return RedirectToAction("Index");
                }

                var result = await _userManagementService.SearchUsersAsync(searchTerm, pageNumber, pageSize);
                var viewModel = new UsersListViewModel
                {
                    Users = [.. result.Users],
                    TotalCount = result.TotalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = result.TotalPages,
                    SearchTerm = searchTerm
                };

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                ShowErrorMessage("Error searching users");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Filter users by role
        /// </summary>
        [HttpGet("by-role/{role}")]
        public async Task<IActionResult> ByRole(UserRole role, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                SetPageTitle($"Users - {role}");
                SetBreadcrumbs( "Users", role.ToString());

                var result = await _userManagementService.GetUsersByRoleAsync(role, pageNumber, pageSize);
                var viewModel = new UsersListViewModel
                {
                    Users = result.Users.ToList(),
                    TotalCount = result.TotalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = result.TotalPages,
                    FilterRole = role.ToString()
                };

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering users by role");
                ShowErrorMessage("Error filtering users");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// View detailed user information
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> Details(string userId)
        {
            try
            {
                var user = await _userManagementService.GetUserDetailAsync(userId);
                SetPageTitle($"User Details - {user.FullName}");
                SetBreadcrumbs("Users", "Details");

                return View(new UserDetailViewModel
                {
                    User = MapToViewModel(user)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user details for: {UserId}", userId);
                ShowErrorMessage("User not found");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Edit user information
        /// </summary>
        [HttpGet("{userId}/edit")]
        public async Task<IActionResult> Edit(string userId)
        {
            try
            {
                var user = await _userManagementService.GetUserDetailAsync(userId);
                SetPageTitle($"Edit User - {user.FullName}");
                SetBreadcrumbs("Users", "Edit");

                return View(new UserEditViewModel
                {
                    User = MapToViewModel(user),
                    FullName = user.FullName,
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit page for user: {UserId}", userId);
                ShowErrorMessage("User not found");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Update user information
        /// </summary>
        [HttpPost("{userId}/edit")]
        public async Task<IActionResult> Edit(string userId, UserEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ShowErrorMessage("Invalid input");
                    return View(model);
                }

                var request = new UpdateUserRequest
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    UserName = model.UserName,
                    PhoneNumber = model.PhoneNumber
                };

                await _userManagementService.UpdateUserAsync(userId, request);
                ShowSuccessMessage($"User '{model.FullName}' updated successfully");
                return RedirectToAction("Details", new { userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", userId);
                ShowErrorMessage("Error updating user");
                return View(model);
            }
        }

        /// <summary>
        /// Change user role
        /// </summary>
        [HttpPost("{userId}/change-role")]
        public async Task<IActionResult> ChangeRole(string userId, UserRole newRole)
        {
            try
            {
                await _userManagementService.ChangeUserRoleAsync(userId, newRole);
                ShowSuccessMessage($"User role changed to {newRole}");
                return RedirectToAction("Details", new { userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing user role: {UserId}", userId);
                ShowErrorMessage("Error changing user role");
                return RedirectToAction("Details", new { userId });
            }
        }

        /// <summary>
        /// Suspend user account
        /// </summary>
        [HttpPost("{userId}/suspend")]
        public async Task<IActionResult> Suspend(string userId, SuspendUserRequest model)
        {
            try
            {
                await _userManagementService.SuspendUserAsync(userId, model.Reason);
                ShowSuccessMessage("User account suspended");
                return RedirectToAction("Details", new { userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending user: {UserId}", userId);
                ShowErrorMessage("Error suspending user");
                return RedirectToAction("Details", new { userId });
            }
        }

        /// <summary>
        /// Reactivate user account
        /// </summary>
        [HttpPost("{userId}/reactivate")]
        public async Task<IActionResult> Reactivate(string userId)
        {
            try
            {
                await _userManagementService.ReactivateUserAsync(userId);
                ShowSuccessMessage("User account reactivated");
                return RedirectToAction("Details", new { userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating user: {UserId}", userId);
                ShowErrorMessage("Error reactivating user");
                return RedirectToAction("Details", new { userId });
            }
        }

        /// <summary>
        /// Delete user account
        /// </summary>
        [HttpPost("{userId}/delete")]
        public async Task<IActionResult> Delete(string userId)
        {
            try
            {
                await _userManagementService.DeleteUserAsync(userId);
                ShowSuccessMessage("User account deleted");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                ShowErrorMessage("Error deleting user");
                return RedirectToAction("Details", new { userId });
            }
        }

        /// <summary>
        /// Reset user password
        /// </summary>
        [HttpPost("{userId}/reset-password")]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            try
            {
                await _userManagementService.ResetUserPasswordAsync(userId);
                ShowSuccessMessage("User password has been reset");
                return RedirectToAction("Details", new { userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user: {UserId}", userId);
                ShowErrorMessage("Error resetting password");
                return RedirectToAction("Details", new { userId });
            }
        }

        /// <summary>
        /// Lock user account temporarily
        /// </summary>
        [HttpPost("{userId}/lock")]
        public async Task<IActionResult> LockAccount(string userId, int durationMinutes = 30)
        {
            try
            {
                await _userManagementService.LockUserAccountAsync(userId, durationMinutes);
                ShowSuccessMessage($"User account locked for {durationMinutes} minutes");
                return RedirectToAction("Details", new { userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking user account: {UserId}", userId);
                ShowErrorMessage("Error locking user account");
                return RedirectToAction("Details", new { userId });
            }
        }

        /// <summary>
        /// Unlock user account
        /// </summary>
        [HttpPost("{userId}/unlock")]
        public async Task<IActionResult> UnlockAccount(string userId)
        {
            try
            {
                await _userManagementService.UnlockUserAccountAsync(userId);
                ShowSuccessMessage("User account unlocked");
                return RedirectToAction("Details", new { userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user account: {UserId}", userId);
                ShowErrorMessage("Error unlocking user account");
                return RedirectToAction("Details", new { userId });
            }
        }

        /// <summary>
        /// Display user statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> Statistics()
        {
            try
            {
                SetPageTitle("User Statistics");
                SetBreadcrumbs("User Statistics");

                var stats = await _userManagementService.GetUserStatisticsAsync();
                
                // Map DTO to ViewModel
                var viewModel = new UserStatisticsViewModel
                {
                    TotalUsers = stats.TotalUsers,
                    ActiveUsers = stats.TotalClients + stats.TotalTrainers, // Active users are clients and trainers
                    VerifiedUsers = stats.VerifiedTrainers,
                    LockedUsers = stats.SuspendedUsers,
                    ClientCount = stats.TotalClients,
                    TrainerCount = stats.TotalTrainers,
                    AdminCount = stats.TotalAdmins,
                    EmailConfirmedCount = stats.TotalUsers, // Assuming all users have confirmed emails
                    NewUsersThisMonth = stats.NewUsersThisMonth,
                    ActiveLastSevenDays = stats.ActiveUsersLastWeek,
                    InactiveOverThirtyDays = 0, // This would need to be calculated from the service
                    TopActiveUsers = [] // Empty for now
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user statistics");
                ShowErrorMessage("Error loading statistics");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Export users data
        /// </summary>
        [HttpGet("export")]
        public async Task<IActionResult> Export(string format = "csv")
        {
            try
            {
                var fileData = await _userManagementService.ExportUsersAsync(format);
                var contentType = format.Equals("csv", StringComparison.CurrentCultureIgnoreCase) ? "text/csv" : "application/json";
                var fileName = $"users_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format}";

                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting users data");
                ShowErrorMessage("Error exporting data");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Export users as CSV
        /// </summary>
        [HttpGet("export-csv")]
        public async Task<IActionResult> ExportCsv()
        {
            try
            {
                var fileData = await _userManagementService.ExportUsersAsync("csv");
                var fileName = $"users_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

                return File(fileData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting CSV");
                ShowErrorMessage("Error exporting CSV");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Export users as Excel
        /// </summary>
        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportExcel()
        {
            try
            {
                var fileData = await _userManagementService.ExportUsersAsync("excel");
                var fileName = $"users_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

                return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting Excel");
                ShowErrorMessage("Error exporting Excel");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Export users as PDF
        /// </summary>
        [HttpGet("export-pdf")]
        public async Task<IActionResult> ExportPdf()
        {
            try
            {
                var fileData = await _userManagementService.ExportUsersAsync("pdf");
                var fileName = $"users_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";

                return File(fileData, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting PDF");
                ShowErrorMessage("Error exporting PDF");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Export user statistics as PDF
        /// </summary>
        [HttpGet("statistics/export-pdf")]
        public async Task<IActionResult> ExportStatisticsPdf()
        {
            try
            {
                var fileData = await _userManagementService.ExportStatisticsPdfAsync();
                var fileName = $"user_statistics_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";

                return File(fileData, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting statistics PDF");
                ShowErrorMessage("Error exporting statistics report");
                return RedirectToAction("Statistics");
            }
        }

        /// <summary>
        /// Verify trainer account
        /// </summary>
        [HttpPost("{userId}/verify-trainer")]
        public async Task<IActionResult> VerifyTrainer(string userId)
        {
            try
            {
                await _userManagementService.VerifyTrainerAsync(userId);
                ShowSuccessMessage("Trainer account verified");
                return RedirectToAction("Details", new { userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying trainer: {UserId}", userId);
                ShowErrorMessage("Error verifying trainer");
                return RedirectToAction("Details", new { userId });
            }
        }

        /// <summary>
        /// View pending trainer verifications
        /// </summary>
        [HttpGet("trainers/pending")]
        public async Task<IActionResult> PendingTrainerVerifications()
        {
            try
            {
                SetPageTitle("Pending Trainer Verifications");
                SetBreadcrumbs("Trainers", "Pending Verifications");

                var pendingTrainers = await _userManagementService.GetPendingTrainerVerificationsAsync();
                return View(pendingTrainers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading pending trainer verifications");
                ShowErrorMessage("Error loading pending verifications");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Bulk operations for users
        /// </summary>
        [HttpPost("bulk-action")]
        public async Task<IActionResult> BulkAction(string action, List<string> userIds, UserRole? newRole = null)
        {
            try
            {
                if (!userIds.Any())
                {
                    ShowErrorMessage("No users selected");
                    return RedirectToAction("Index");
                }

                switch (action.ToLower())
                {
                    case "change-role":
                        if (!newRole.HasValue)
                        {
                            ShowErrorMessage("Please select a role");
                            return RedirectToAction("Index");
                        }

                        var bulkRequest = new BulkRoleUpdateRequest
                        {
                            UserIds = userIds,
                            NewRole = newRole.Value
                        };

                        var result = await _userManagementService.BulkUpdateUserRolesAsync(bulkRequest);
                        if (result.IsSuccess)
                        {
                            ShowSuccessMessage($"Successfully updated {result.SuccessCount} users");
                        }
                        else
                        {
                            ShowWarningMessage($"Updated {result.SuccessCount} users, {result.FailureCount} failed");
                        }
                        break;

                    default:
                        ShowErrorMessage("Unknown action");
                        break;
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk action");
                ShowErrorMessage("Error performing bulk action");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Helper method to map from Application DTO to ViewModel
        /// </summary>
        private static UserDetailResponse MapToViewModel(AppUserDetailResponse user)
        {
            return new UserDetailResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                ProfilePhotoUrl = user.ProfilePhotoUrl,
                Role = user.Role,
                IsVerified = user.IsVerified,
                IsLockedOut = user.IsLockedOut,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount,
                Status = user.Status,
                CurrentRoles = user.CurrentRoles.ToList()
            };
        }
    }
}
