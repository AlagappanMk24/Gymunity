using Gymunity.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Gymunity.APIs.Services
{
    /// <summary>
    /// Helper service to get admin user(s) for notification purposes
    /// </summary>
    public class AdminUserResolverService(UserManager<AppUser> userManager, ILogger<AdminUserResolverService> logger)
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly ILogger<AdminUserResolverService> _logger = logger;

        /// <summary>
        /// Get the primary admin user (usually the first admin)
        /// </summary>
        public async Task<AppUser?> GetPrimaryAdminAsync()
        {
            try
            {
                var allUsers = _userManager.Users.ToList();
                var admin = allUsers.FirstOrDefault();
                
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found");
                    return null;
                }

                // Check if user has Admin role
                var roles = await _userManager.GetRolesAsync(admin);
                if (!roles.Contains("Admin"))
                {
                    // Try to find actual admin
                    foreach (var user in allUsers)
                    {
                        var userRoles = await _userManager.GetRolesAsync(user);
                        if (userRoles.Contains("Admin"))
                        {
                            return user;
                        }
                    }
                }

                return admin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving primary admin user");
                return null;
            }
        }

        /// <summary>
        /// Get all admin users
        /// </summary>
        public async Task<IEnumerable<AppUser>> GetAllAdminsAsync()
        {
            try
            {
                var allUsers = _userManager.Users.ToList();
                var admins = new List<AppUser>();

                foreach (var user in allUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin"))
                    {
                        admins.Add(user);
                    }
                }

                return admins;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin users");
                return new List<AppUser>();
            }
        }

        /// <summary>
        /// Get admin user by ID
        /// </summary>
        public async Task<AppUser?> GetAdminByIdAsync(string adminUserId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(adminUserId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", adminUserId);
                    return null;
                }

                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Admin"))
                {
                    _logger.LogWarning("User {UserId} is not an admin", adminUserId);
                    return null;
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin by ID: {AdminUserId}", adminUserId);
                return null;
            }
        }
    }
}