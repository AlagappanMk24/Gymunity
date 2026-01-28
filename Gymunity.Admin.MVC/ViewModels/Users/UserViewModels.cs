using Gymunity.Application.DTOs.Admin;
using Gymunity.Domain.Enums;

namespace Gymunity.Admin.MVC.ViewModels.Users
{
    /// <summary>
    /// ViewModel for users list page
    /// </summary>
    public class UsersListViewModel
    {
        public List<UserManagementDto> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public string? SearchTerm { get; set; }
        public string? FilterRole { get; set; }
    }

    /// <summary>
    /// ViewModel for user detail page
    /// </summary>
    public class UserDetailViewModel
    {
        public UserDetailResponse User { get; set; } = null!;
    }

    /// <summary>
    /// ViewModel for editing user
    /// </summary>
    public class EditUserViewModel
    {
        public UserDetailResponse User { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// ViewModel for editing user (alias for Edit view compatibility)
    /// </summary>
    public class UserEditViewModel : EditUserViewModel
    {
    }

    /// <summary>
    /// ViewModel for user card display
    /// </summary>
    public class UserItemViewModel
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public UserRole Role { get; set; }
        public bool IsVerified { get; set; }
        public bool IsLockedOut { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int LoginCount { get; set; }
    }

    /// <summary>
    /// ViewModel for user statistics
    /// </summary>
    public class UserStatisticsViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int VerifiedUsers { get; set; }
        public int LockedUsers { get; set; }
        public int ClientCount { get; set; }
        public int TrainerCount { get; set; }
        public int AdminCount { get; set; }
        public int EmailConfirmedCount { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int ActiveLastSevenDays { get; set; }
        public int InactiveOverThirtyDays { get; set; }
        public List<UserStatisticsItemViewModel> TopActiveUsers { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for individual user in statistics
    /// </summary>
    public class UserStatisticsItemViewModel
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int LoginCount { get; set; }
    }

    /// <summary>
    /// View model for user details (maps from Application DTO)
    /// </summary>
    public class UserDetailResponse
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string? Bio { get; set; }
        public UserRole Role { get; set; }
        public bool IsVerified { get; set; }
        public bool IsLockedOut { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
        public string Status { get; set; } = "Active";
        public List<string> CurrentRoles { get; set; } = new();
    }
}