using Gymunity.Domain.Enums;

namespace Gymunity.Application.DTOs.Admin
{
    // ============== Request DTOs ==============

    public class UpdateUserRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? EmailConfirmed { get; set; }
    }

    public class SuspendUserRequest
    {
        public string UserId { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public int? DurationDays { get; set; }
    }

    public class BulkRoleUpdateRequest
    {
        public List<string> UserIds { get; set; } = new();
        public UserRole NewRole { get; set; }
    }

    // ============== Response DTOs ==============

    public class UserManagementListResponse
    {
        public IEnumerable<UserManagementDto> Users { get; set; } = new List<UserManagementDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class UserManagementDto
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool EmailConfirmed { get; set; }
        public bool IsVerified { get; set; }
        public bool IsLockedOut { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string Status { get; set; } = "Active"; // Active, Suspended, Deleted
    }

    public class UserDetailResponse
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public UserRole Role { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool IsVerified { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? StripeCustomerId { get; set; }
        public string? StripeConnectAccountId { get; set; }
        public int AccessFailedCount { get; set; }
        public IEnumerable<string> CurrentRoles { get; set; } = new List<string>();
        public string Status { get; set; } = "Active";
        public string? SuspensionReason { get; set; }
        public DateTime? SuspendedAt { get; set; }
    }

    public class UserActivityLogResponse
    {
        public int Id { get; set; }
        public string Action { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class UserStatisticsResponse
    {
        public int TotalUsers { get; set; }
        public int TotalClients { get; set; }
        public int TotalTrainers { get; set; }
        public int TotalAdmins { get; set; }
        public int VerifiedTrainers { get; set; }
        public int UnverifiedTrainers { get; set; }
        public int SuspendedUsers { get; set; }
        public int ActiveUsersLastWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
        public double ClientTrainerRatio { get; set; }
    }

    public class PendingTrainerVerificationResponse
    {
        public string UserId { get; set; } = null!;
        public string TrainerName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Handle { get; set; } = null!;
        public string Bio { get; set; } = null!;
        public int YearsExperience { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime RequestedAt { get; set; }
    }

    public class BulkOperationResponse
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> FailedUserIds { get; set; } = new();
        public string Message { get; set; } = null!;
        public bool IsSuccess => FailureCount == 0;
    }

    public class RoleChangeAuditDto
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string OldRole { get; set; } = null!;
        public string NewRole { get; set; } = null!;
        public string ChangedByAdminId { get; set; } = null!;
        public DateTime ChangedAt { get; set; }
    }
}