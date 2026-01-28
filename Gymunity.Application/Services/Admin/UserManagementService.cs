using ClosedXML.Excel;
using Gymunity.Application.Contracts.Admin;
using Gymunity.Application.DTOs.Admin;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;

namespace Gymunity.Application.Services.Admin
{
    public class UserManagementService(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<UserManagementService> logger) : IUserManagementService
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly ILogger<UserManagementService> _logger = logger;

        // ✅ Observable events for notification handlers
        public event Func<string, Task>? UserSuspendedAsync;
        public event Func<string, Task>? UserReactivatedAsync;
        public event Func<string, Task>? UserDeletedAsync;
        public event Func<string, UserRole, Task>? UserRoleChangedAsync;

        public async Task<UserManagementListResponse> GetAllUsersAsync(int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var query = _userManager.Users.AsQueryable();
                var totalCount = await query.CountAsync();
                
                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var userDtos = new List<UserManagementDto>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userDtos.Add(MapToUserManagementDto(user, roles));
                }

                return new UserManagementListResponse
                {
                    Users = userDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                throw;
            }
        }

        public async Task<UserManagementListResponse> SearchUsersAsync(string searchTerm, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var query = _userManager.Users
                    .Where(u => u.Email.Contains(searchTerm) ||
                               u.UserName.Contains(searchTerm) ||
                               u.FullName.Contains(searchTerm))
                    .AsQueryable();

                var totalCount = await query.CountAsync();

                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var userDtos = new List<UserManagementDto>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userDtos.Add(MapToUserManagementDto(user, roles));
                }

                return new UserManagementListResponse
                {
                    Users = userDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users with term: {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<UserManagementListResponse> GetUsersByRoleAsync(UserRole role, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var roleName = role.ToString();
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);

                var totalCount = usersInRole.Count;
                var users = usersInRole
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var userDtos = new List<UserManagementDto>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userDtos.Add(MapToUserManagementDto(user, roles));
                }

                return new UserManagementListResponse
                {
                    Users = userDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users by role: {Role}", role);
                throw;
            }
        }

        public async Task<UserDetailResponse> GetUserDetailAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                var roles = await _userManager.GetRolesAsync(user);

                return new UserDetailResponse
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    Role = user.Role,
                    ProfilePhotoUrl = user.ProfilePhotoUrl,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    IsVerified = user.IsVerified,
                    IsLockedOut = user.LockoutEnd > DateTimeOffset.UtcNow,
                    LockoutEnd = user.LockoutEnd,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    StripeCustomerId = user.StripeCustomerId,
                    StripeConnectAccountId = user.StripeConnectAccountId,
                    AccessFailedCount = user.AccessFailedCount,
                    CurrentRoles = roles
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user details for: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                if (!string.IsNullOrWhiteSpace(request.FullName))
                    user.FullName = request.FullName;

                if (!string.IsNullOrWhiteSpace(request.Email) && user.Email != request.Email)
                {
                    var emailExists = await _userManager.FindByEmailAsync(request.Email);
                    if (emailExists != null)
                        throw new Exception("Email is already in use");
                    user.Email = request.Email;
                    user.NormalizedEmail = _userManager.NormalizeName(request.Email);
                }

                if (!string.IsNullOrWhiteSpace(request.UserName) && user.UserName != request.UserName)
                {
                    var userNameExists = await _userManager.FindByNameAsync(request.UserName);
                    if (userNameExists != null)
                        throw new Exception("Username is already in use");
                    user.UserName = request.UserName;
                    user.NormalizedUserName = _userManager.NormalizeName(request.UserName);
                }

                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                    user.PhoneNumber = request.PhoneNumber;

                if (request.EmailConfirmed.HasValue)
                    user.EmailConfirmed = request.EmailConfirmed.Value;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to update user: {errors}");
                }

                _logger.LogInformation("User {UserId} updated successfully", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ChangeUserRoleAsync(string userId, UserRole newRole)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    throw new Exception("User not found");

                var currentRoles = await _userManager.GetRolesAsync(user);

                // Remove old roles
                foreach (var role in currentRoles)
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }

                // Add new role
                var newRoleName = newRole.ToString();
                var roleExists = await _roleManager.RoleExistsAsync(newRoleName);
                if (!roleExists)
                {
                    throw new Exception($"Role {newRoleName} does not exist");
                }

                var result = await _userManager.AddToRoleAsync(user, newRoleName);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to change role: {errors}");
                }

                user.Role = newRole;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User {UserId} role changed to {NewRole}", userId, newRole);

                // ✅ Raise event for notification handlers
                if (UserRoleChangedAsync != null)
                {
                    try
                    {
                        await UserRoleChangedAsync(userId, newRole);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Event notification failed for role change {UserId}", userId);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing user role: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> SuspendUserAsync(string userId, string reason)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    throw new Exception("User not found");

                // Lock the account
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

                _logger.LogWarning("User {UserId} suspended with reason: {Reason}", userId, reason);

                // ✅ Raise event for notification handlers
                if (UserSuspendedAsync != null)
                {
                    try
                    {
                        await UserSuspendedAsync(userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Event notification failed for user suspension {UserId}", userId);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ReactivateUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    throw new Exception("User not found");

                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                await _userManager.ResetAccessFailedCountAsync(user);

                _logger.LogInformation("User {UserId} reactivated", userId);

                // ✅ Raise event for notification handlers
                if (UserReactivatedAsync != null)
                {
                    try
                    {
                        await UserReactivatedAsync(userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Event notification failed for user reactivation {UserId}", userId);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    throw new Exception("User not found");

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to delete user: {errors}");
                }

                _logger.LogWarning("User {UserId} deleted", userId);

                // ✅ Raise event for notification handlers
                if (UserDeletedAsync != null)
                {
                    try
                    {
                        await UserDeletedAsync(userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Event notification failed for user deletion {UserId}", userId);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ResetUserPasswordAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    throw new Exception("User not found");

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var tempPassword = GenerateTemporaryPassword();

                var result = await _userManager.ResetPasswordAsync(user, token, tempPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to reset password: {errors}");
                }

                _logger.LogInformation("Password reset for user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting user password: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> LockUserAccountAsync(string userId, int durationMinutes)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    throw new Exception("User not found");

                var lockoutEnd = DateTimeOffset.UtcNow.AddMinutes(durationMinutes);
                await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);

                _logger.LogWarning("User {UserId} locked for {Minutes} minutes", userId, durationMinutes);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking user account: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UnlockUserAccountAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    throw new Exception("User not found");

                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);

                _logger.LogInformation("User {UserId} unlocked", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user account: {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<UserActivityLogResponse>> GetUserActivityLogsAsync(string userId, int pageSize = 50)
        {
            try
            {
                // This would typically come from an activity log table
                // For now, returning empty list
                return new List<UserActivityLogResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user activity logs for: {UserId}", userId);
                throw;
            }
        }

        public async Task<UserStatisticsResponse> GetUserStatisticsAsync()
        {
            try
            {
                var allUsers = await _userManager.Users.ToListAsync();
                var totalUsers = allUsers.Count;
                var clients = allUsers.Count(u => u.Role == UserRole.Client);
                var trainers = allUsers.Count(u => u.Role == UserRole.Trainer);
                var admins = allUsers.Count(u => u.Role == UserRole.Admin);

                return new UserStatisticsResponse
                {
                    TotalUsers = totalUsers,
                    TotalClients = clients,
                    TotalTrainers = trainers,
                    TotalAdmins = admins,
                    VerifiedTrainers = 0, // Would come from TrainerProfile table
                    UnverifiedTrainers = trainers,
                    SuspendedUsers = allUsers.Count(u => u.LockoutEnd > DateTimeOffset.UtcNow),
                    ActiveUsersLastWeek = allUsers.Count(u => u.LastLoginAt > DateTime.UtcNow.AddDays(-7)),
                    NewUsersThisMonth = allUsers.Count(u => u.CreatedAt > DateTime.UtcNow.AddMonths(-1)),
                    ClientTrainerRatio = trainers > 0 ? (double)clients / trainers : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user statistics");
                throw;
            }
        }

        public async Task<IEnumerable<PendingTrainerVerificationResponse>> GetPendingTrainerVerificationsAsync()
        {
            try
            {
                // This would query TrainerProfile where IsVerified = false
                return new List<PendingTrainerVerificationResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending trainer verifications");
                throw;
            }
        }

        public async Task<bool> VerifyTrainerAsync(string trainerId)
        {
            try
            {
                var trainer = await _userManager.FindByIdAsync(trainerId);
                if (trainer == null || trainer.Role != UserRole.Trainer)
                    throw new Exception("Trainer not found");

                trainer.IsVerified = true;
                var result = await _userManager.UpdateAsync(trainer);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to verify trainer: {errors}");
                }

                _logger.LogInformation("Trainer {TrainerId} verified", trainerId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying trainer: {TrainerId}", trainerId);
                throw;
            }
        }

        public async Task<bool> RejectTrainerVerificationAsync(string trainerId, string reason)
        {
            try
            {
                var trainer = await _userManager.FindByIdAsync(trainerId);
                if (trainer == null)
                    throw new Exception("Trainer not found");

                _logger.LogInformation("Trainer {TrainerId} verification rejected. Reason: {Reason}", trainerId, reason);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting trainer verification: {TrainerId}", trainerId);
                throw;
            }
        }

        public async Task<BulkOperationResponse> BulkUpdateUserRolesAsync(BulkRoleUpdateRequest request)
        {
            var response = new BulkOperationResponse
            {
                SuccessCount = 0,
                FailureCount = 0
            };

            try
            {
                foreach (var userId in request.UserIds)
                {
                    try
                    {
                        await ChangeUserRoleAsync(userId, request.NewRole);
                        response.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating role for user: {UserId}", userId);
                        response.FailureCount++;
                        response.FailedUserIds.Add(userId);
                    }
                }

                response.Message = $"Bulk role update completed. Success: {response.SuccessCount}, Failed: {response.FailureCount}";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk role update operation");
                throw;
            }
        }

        //public async Task<byte[]> ExportUsersAsync(string format = "csv")
        //{
        //    try
        //    {
        //        var users = await _userManager.Users.ToListAsync();

        //        if (format.ToLower() == "csv")
        //        {
        //            return ExportToCSV(users);
        //        }

        //        throw new Exception("Unsupported export format");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error exporting users");
        //        throw;
        //    }
        //}

        //private byte[] ExportToCSV(List<AppUser> users)
        //{
        //    var csv = "Id,FullName,Email,UserName,Role,EmailConfirmed,IsVerified,CreatedAt,LastLoginAt\n";

        //    foreach (var user in users)
        //    {
        //        csv += $"\"{user.Id}\",\"{user.FullName}\",\"{user.Email}\",\"{user.UserName}\",\"{user.Role}\",{user.EmailConfirmed},{user.IsVerified},\"{user.CreatedAt}\",\"{user.LastLoginAt}\"\n";
        //    }

        //    return System.Text.Encoding.UTF8.GetBytes(csv);
        //}

        public async Task<byte[]> ExportUsersAsync(string format = "csv")
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();

                return format.ToLower() switch
                {
                    "csv" => ExportToCSV(users),
                    "excel" => ExportToExcel(users),
                    "pdf" => ExportToPDF(users),
                    _ => throw new Exception("Unsupported export format")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting users");
                throw;
            }
        }
        private byte[] ExportToCSV(List<AppUser> users)
        {
            var csv = new StringBuilder();

            // Add headers
            csv.AppendLine("ID,Full Name,Email,Username,Phone Number,Role,Email Confirmed,Verified,Locked Out,Created At,Last Login,Status");

            foreach (var user in users)
            {
                var roles = _userManager.GetRolesAsync(user).Result;
                var status = user.LockoutEnd > DateTimeOffset.UtcNow ? "Locked" : "Active";

                csv.AppendLine($"\"{user.Id}\",\"{user.FullName}\",\"{user.Email}\",\"{user.UserName}\",\"{user.PhoneNumber ?? "N/A"}\",\"{user.Role}\",{user.EmailConfirmed},{user.IsVerified},{user.LockoutEnd > DateTimeOffset.UtcNow},\"{user.CreatedAt}\",\"{user.LastLoginAt?.ToString() ?? "Never"}\",\"{status}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }
        private byte[] ExportToExcel(List<AppUser> users)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Users");

                // Add headers
                var headers = new[]
                {
                    "ID", "Full Name", "Email", "Username", "Phone Number", "Role",
                    "Email Confirmed", "Verified", "Locked Out", "Created At", "Last Login", "Status"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                // Add data
                int row = 2;
                foreach (var user in users)
                {
                    var roles = _userManager.GetRolesAsync(user).Result;
                    var status = user.LockoutEnd > DateTimeOffset.UtcNow ? "Locked" : "Active";

                    worksheet.Cell(row, 1).Value = user.Id;
                    worksheet.Cell(row, 2).Value = user.FullName;
                    worksheet.Cell(row, 3).Value = user.Email;
                    worksheet.Cell(row, 4).Value = user.UserName;
                    worksheet.Cell(row, 5).Value = user.PhoneNumber ?? "N/A";
                    worksheet.Cell(row, 6).Value = user.Role.ToString();
                    worksheet.Cell(row, 7).Value = user.EmailConfirmed ? "Yes" : "No";
                    worksheet.Cell(row, 8).Value = user.IsVerified ? "Yes" : "No";
                    worksheet.Cell(row, 9).Value = user.LockoutEnd > DateTimeOffset.UtcNow ? "Yes" : "No";
                    worksheet.Cell(row, 10).Value = user.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cell(row, 11).Value = user.LastLoginAt?.ToString("yyyy-MM-dd HH:mm") ?? "Never";
                    worksheet.Cell(row, 12).Value = status;

                    // Style for status column
                    var statusCell = worksheet.Cell(row, 12);
                    if (status == "Locked")
                    {
                        statusCell.Style.Fill.BackgroundColor = XLColor.Red;
                        statusCell.Style.Font.FontColor = XLColor.White;
                    }
                    else
                    {
                        statusCell.Style.Fill.BackgroundColor = XLColor.Green;
                        statusCell.Style.Font.FontColor = XLColor.White;
                    }

                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using var memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Excel export");
                throw;
            }
        }
        private byte[] ExportToPDF(List<AppUser> users)
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community; // Free license for open source

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);

                        page.Header().Text("User Management Report")
                            .FontSize(20).Bold().AlignCenter();

                        page.Content().Column(column =>
                        {
                            // Summary
                            column.Item().Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}")
                                .FontSize(10).Italic().AlignRight();

                            var activeUsers = users.Count(u => u.LockoutEnd <= DateTimeOffset.UtcNow);
                            var lockedUsers = users.Count(u => u.LockoutEnd > DateTimeOffset.UtcNow);

                            column.Item().PaddingVertical(10).Text($"Total Users: {users.Count} | Active: {activeUsers} | Locked: {lockedUsers}")
                                .FontSize(12).SemiBold();

                            // Table
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2); // Name
                                    columns.RelativeColumn(2); // Email
                                    columns.RelativeColumn();  // Role
                                    columns.RelativeColumn();  // Status
                                    columns.RelativeColumn();  // Email Confirmed
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Text("Name").Bold();
                                    header.Cell().Text("Email").Bold();
                                    header.Cell().Text("Role").Bold();
                                    header.Cell().Text("Status").Bold();
                                    header.Cell().Text("Email Confirmed").Bold();
                                });

                                // Data
                                foreach (var user in users)
                                {
                                    var status = user.LockoutEnd > DateTimeOffset.UtcNow ? "Locked" : "Active";

                                    table.Cell().Text(user.FullName);
                                    table.Cell().Text(user.Email);
                                    table.Cell().Text(user.Role.ToString());
                                    table.Cell().Text(status).FontColor(status == "Locked" ? Colors.Red.Medium : Colors.Green.Medium);
                                    table.Cell().Text(user.EmailConfirmed ? "Yes" : "No");
                                }
                            });
                        });

                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                    });
                });

                using var memoryStream = new MemoryStream();
                document.GeneratePdf(memoryStream);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PDF export with QuestPDF");
                throw;
            }
        }
        public async Task<byte[]> ExportStatisticsPdfAsync()
        {
            try
            {
                var stats = await GetUserStatisticsAsync();
                var users = await _userManager.Users.ToListAsync();
                var topUsers = await GetTopActiveUsersAsync(10);

                QuestPDF.Settings.License = LicenseType.Community;

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Column(column =>
                        {
                            column.Item().Text("User Statistics Report")
                                .FontSize(20).Bold().FontColor(Colors.Blue.Darken2).AlignCenter();

                            column.Item().Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}")
                                .FontSize(9).Italic().AlignRight();

                            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                        });

                        page.Content().PaddingVertical(10).Column(column =>
                        {
                            // Executive Summary
                            column.Item().Background(Colors.Grey.Lighten5).Padding(15).Column(summaryColumn =>
                            {
                                summaryColumn.Item().Text("EXECUTIVE SUMMARY").FontSize(14).Bold().FontColor(Colors.Blue.Medium);

                                summaryColumn.Item().PaddingTop(10).Row(row =>
                                {
                                    row.RelativeItem().Background(Colors.Blue.Lighten5).Padding(10).Column(subCol =>
                                    {
                                        subCol.Item().Text("Total Users").FontSize(12).SemiBold();
                                        subCol.Item().Text($"{stats.TotalUsers}").FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                                    });

                                    row.RelativeItem().Background(Colors.Green.Lighten5).Padding(10).Column(subCol =>
                                    {
                                        subCol.Item().Text("Active Users").FontSize(12).SemiBold();
                                        subCol.Item().Text($"{stats.ActiveUsersLastWeek}").FontSize(18).Bold().FontColor(Colors.Green.Darken3);
                                    });

                                    row.RelativeItem().Background(Colors.Orange.Lighten5).Padding(10).Column(subCol =>
                                    {
                                        subCol.Item().Text("New This Month").FontSize(12).SemiBold();
                                        subCol.Item().Text($"{stats.NewUsersThisMonth}").FontSize(18).Bold().FontColor(Colors.Orange.Darken3);
                                    });

                                    row.RelativeItem().Background(Colors.Red.Lighten5).Padding(10).Column(subCol =>
                                    {
                                        subCol.Item().Text("Suspended").FontSize(12).SemiBold();
                                        subCol.Item().Text($"{stats.SuspendedUsers}").FontSize(18).Bold().FontColor(Colors.Red.Darken3);
                                    });
                                });
                            });

                            // Role Distribution
                            column.Item().PaddingTop(20).Text("ROLE DISTRIBUTION").FontSize(14).Bold().FontColor(Colors.Blue.Medium);
                            column.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                // Headers
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Role").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Count").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Percentage").Bold();
                                });

                                // Data
                                var roles = new[]
                                {
                            ("Clients", stats.TotalClients),
                            ("Trainers", stats.TotalTrainers),
                            ("Admins", stats.TotalAdmins)
                        };

                                foreach (var (roleName, count) in roles)
                                {
                                    var percentage = stats.TotalUsers > 0 ? (count * 100.0 / stats.TotalUsers) : 0;

                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(roleName);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(count.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{percentage:F1}%");
                                }

                                // Total
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("Total").Bold();
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text(stats.TotalUsers.ToString()).Bold();
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("100%").Bold();
                            });

                            // Activity Metrics
                            column.Item().PaddingTop(20).Text("ACTIVITY METRICS").FontSize(14).Bold().FontColor(Colors.Blue.Medium);
                            column.Item().PaddingTop(5).Grid(grid =>
                            {
                                grid.Columns(2);
                                grid.Item().Background(Colors.Green.Lighten5).Padding(10).Column(subCol =>
                                {
                                    subCol.Item().Text("Active (Last 7 Days)").FontSize(12).SemiBold();
                                    subCol.Item().Text($"{stats.ActiveUsersLastWeek}").FontSize(16).Bold().FontColor(Colors.Green.Darken3);
                                });
                                grid.Item().Background(Colors.Orange.Lighten5).Padding(10).Column(subCol =>
                                {
                                    subCol.Item().Text("Inactive (30+ Days)").FontSize(12).SemiBold();
                                    // Calculate inactive users (users who haven't logged in in 30+ days)
                                    var inactiveCount = users.Count(u => !u.LastLoginAt.HasValue || u.LastLoginAt < DateTime.UtcNow.AddDays(-30));
                                    subCol.Item().Text($"{inactiveCount}").FontSize(16).Bold().FontColor(Colors.Orange.Darken3);
                                });
                                grid.Item().Background(Colors.Blue.Lighten5).Padding(10).Column(subCol =>
                                {
                                    subCol.Item().Text("Email Confirmed").FontSize(12).SemiBold();
                                    var emailConfirmedCount = users.Count(u => u.EmailConfirmed);
                                    subCol.Item().Text($"{emailConfirmedCount}").FontSize(16).Bold().FontColor(Colors.Blue.Darken3);
                                });
                                grid.Item().Background(Colors.Purple.Lighten5).Padding(10).Column(subCol =>
                                {
                                    subCol.Item().Text("Trainer Verified").FontSize(12).SemiBold();
                                    subCol.Item().Text($"{stats.VerifiedTrainers}/{stats.TotalTrainers}").FontSize(16).Bold().FontColor(Colors.Purple.Darken3);
                                });
                            });

                            // Top Active Users
                            if (topUsers.Any())
                            {
                                column.Item().PageBreak();
                                column.Item().Text("TOP 10 ACTIVE USERS").FontSize(14).Bold().FontColor(Colors.Blue.Medium);
                                column.Item().PaddingTop(5).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3); // Name
                                        columns.RelativeColumn();  // Role
                                        columns.RelativeColumn();  // Last Login
                                        columns.RelativeColumn();  // Login Count
                                        columns.RelativeColumn();  // Created
                                    });

                                    // Headers
                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("User Name").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Role").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Last Login").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Login Count").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Created").Bold();
                                    });

                                    // Data
                                    foreach (var user in topUsers)
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(user.FullName);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(user.Role.ToString());
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(user.LastLoginAt?.ToString("yyyy-MM-dd") ?? "Never");
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(user.LoginCount.ToString());
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(user.CreatedAt.ToString("yyyy-MM-dd"));
                                    }
                                });
                            }

                            column.Item().PaddingTop(20).Text("DATA VISUALIZATION").FontSize(14).Bold().FontColor(Colors.Blue.Medium);
                            column.Item().PaddingTop(5).Text("Role Distribution:").FontSize(11).SemiBold();
                            column.Item().PaddingTop(5).Table(chartTable =>
                            {
                                chartTable.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2); // Role
                                    columns.RelativeColumn(3); // Bar
                                });

                                var maxCount = Math.Max(stats.TotalClients, Math.Max(stats.TotalTrainers, stats.TotalAdmins));

                                var roleData = new[]
                                {
                            ("Clients", stats.TotalClients, Colors.Blue.Medium),
                            ("Trainers", stats.TotalTrainers, Colors.Orange.Medium),
                            ("Admins", stats.TotalAdmins, Colors.Red.Medium)
                        };

                                foreach (var (role, count, color) in roleData)
                                {
                                    var percentage = maxCount > 0 ? (count * 100.0 / maxCount) : 0;
                                    var percentageFloat = (float)percentage;

                                    chartTable.Cell().PaddingVertical(3).Text(role);
                                    chartTable.Cell().PaddingVertical(3).Row(row =>
                                    {
                                        // FIXED: Convert string percentage to float and use correct method
                                        row.RelativeItem().Height(15).Background(color.WithAlpha(0.3f)).Width(percentageFloat);
                                        row.AutoItem().PaddingLeft(5).Text($"{count} ({percentage:F0}%)").FontSize(9);
                                    });
                                }
                            });
                        });

                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.Span("Gymunity User Management System | Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                            text.Span($" | Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
                        });

                    });
                });

                using var memoryStream = new MemoryStream();
                document.GeneratePdf(memoryStream);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting statistics PDF");
                throw;
            }
        }

        // Fix the GetTopActiveUsersAsync method:
        private async Task<List<UserStatisticsItemViewModel>> GetTopActiveUsersAsync(int count)
        {
            try
            {
                var users = await _userManager.Users
                    .Where(u => u.LastLoginAt.HasValue)
                    .OrderByDescending(u => u.LoginCount)
                    .ThenByDescending(u => u.LastLoginAt)
                    .Take(count)
                    .ToListAsync();

                return users.Select(user => new UserStatisticsItemViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    LoginCount = user.LoginCount
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top active users");
                return new List<UserStatisticsItemViewModel>();
            }
        }
        private static UserManagementDto MapToUserManagementDto(AppUser user, IList<string> roles)
        {
            return new UserManagementDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                Role = roles.FirstOrDefault() ?? user.Role.ToString(),
                EmailConfirmed = user.EmailConfirmed,
                IsVerified = user.IsVerified,
                IsLockedOut = user.LockoutEnd > DateTimeOffset.UtcNow,
                ProfilePhotoUrl = user.ProfilePhotoUrl,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Status = user.LockoutEnd > DateTimeOffset.UtcNow ? "Suspended" : "Active"
            };
        }
        private static string GenerateTemporaryPassword()
        {
            return Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }

        public class UserStatisticsItemViewModel
        {
            public string Id { get; set; } = null!;
            public string FullName { get; set; } = null!;
            public UserRole Role { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? LastLoginAt { get; set; }
            public int LoginCount { get; set; }
        }
    }
}