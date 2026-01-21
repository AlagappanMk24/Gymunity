using Gymunity.APIs.Responses;
using Gymunity.APIs.Services;
using Gymunity.Application.Contracts.ExternalServices.Auth;
using Gymunity.Application.Contracts.Services.Identity;
using Gymunity.Application.DTOs.Account;
using Gymunity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gymunity.APIs.Controllers
{
    /// <summary>
    /// Provides API endpoints for user account registration and authentication operations.
    /// </summary>
    /// <remarks>
    /// This controller exposes endpoints for registering new users and authenticating existing
    /// users, including support for standard and Google-based authentication. All endpoints return appropriate HTTP
    /// responses based on the outcome of the requested operation.
    /// </remarks>
    /// <param name="identityService">The identity service used for user registration and login operations.</param>
    /// <param name="accountService">The account service used for user profile management operations.</param>
    /// <param name="passwordService">The password service used for password reset and change operations.</param>
    /// <param name="googleAuthService">The external authentication service used for Google authentication.</param>
    /// <param name="adminNotificationService">The admin notification service used to notify administrators of new registrations.</param>
    /// <param name="adminUserResolver">The service used to resolve administrator users for notifications.</param>
    /// <param name="logger">The logger instance for recording controller operations and errors.</param>
    [Route("api/account")]
    public class AccountController(
        IIdentityService identityService,
        IAccountService accountService,
        IPasswordService passwordService,
        IGoogleAuthService googleAuthService,
        IAdminNotificationService adminNotificationService,
        AdminUserResolverService adminUserResolver,
        ILogger<AccountController> logger) : BaseApiController
    {
        private readonly IIdentityService _identityService = identityService;
        private readonly IAccountService _accountService = accountService;
        private readonly IPasswordService _passwordService = passwordService;
        private readonly IGoogleAuthService _googleAuthService = googleAuthService;
        private readonly IAdminNotificationService _adminNotificationService = adminNotificationService;
        private readonly AdminUserResolverService _adminUserResolver = adminUserResolver;
        private readonly ILogger<AccountController> _logger = logger;

        /// <summary>
        /// Registers a new user account using the provided registration details.
        /// </summary>
        /// <remarks>This endpoint is typically used to create a new user account. The request data is
        /// expected as form data. If registration fails due to validation or other errors, a 400 Bad Request response
        /// is returned with an error message.</remarks>
        /// <param name="request">The registration information submitted by the client. Must include all required user details as specified by
        /// the registration process. Cannot be null.</param>
        /// <returns>An <see cref="ActionResult{AuthResponse}"/> containing the newly created user's information if registration
        /// is successful; otherwise, a bad request response with error details.</returns>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromForm] RegisterRequest request)
        {
            try
            {
                var userResponse = await _identityService.RegisterAsync(request);

                // ✅ Notify admin of new registration
                await NotifyAdminOfRegistrationAsync(userResponse);

                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Authenticates a user with the provided login credentials and returns user information if authentication is
        /// successful.
        /// </summary>
        /// <param name="request">The login credentials submitted by the client. Must include valid username and password values.</param>
        /// <returns>An <see cref="ActionResult{T}"/> containing a <see cref="AuthResponse"/> if authentication succeeds;
        /// otherwise, a bad request response with error details.</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var userResponse = await _identityService.LoginAsync(request);
                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Authenticates a user using Google credentials and returns user information if authentication is successful.
        /// </summary>
        /// <param name="request">The Google authentication request containing the user's credentials. Cannot be null.</param>
        /// <returns>An ActionResult containing the authenticated user's information if successful; otherwise, a BadRequest
        /// result with error details.</returns>
        [HttpPost("google-auth")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> GoogleAuth([FromBody] GoogleAuthRequest request)
        {
            try
            {
                var userResponse = await _googleAuthService.GoogleAuthAsync(request);

                // ✅ Notify admin of new Google registration
                await NotifyAdminOfGoogleRegistrationAsync(userResponse);

                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Updates the authenticated user's profile with the specified information.
        /// </summary>
        /// <remarks>This endpoint requires authentication. Only the currently authenticated user's
        /// profile can be updated using this method.</remarks>
        /// <param name="request">An <see cref="UpdateProfileRequest"/> object containing the new profile details to apply. Cannot be null.</param>
        /// <returns>An <see cref="ActionResult{UserResponse}"/> containing the updated user profile if the update is successful;
        /// otherwise, a bad request response with error details.</returns>
        [HttpPut("update-profile")]
        [Authorize]
        public async Task<ActionResult<AuthResponse>> UpdateProfile([FromForm] UpdateProfileRequest request)
        {
            try
            {
                var userResponse = await _accountService.UpdateProfileAsync(GetUserId()!, request);
                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }

        }

        /// <summary>
        /// Changes the current user's password using the specified password change request.
        /// </summary>
        /// <remarks>This endpoint requires authentication. The user must provide their current password
        /// and a valid new password as specified by the application's password policy.</remarks>
        /// <param name="request">An object containing the current password and the new password to set. Must not be null.</param>
        /// <returns>An <see cref="ActionResult{UserResponse}"/> containing a UserResponse if the password change is successful; otherwise, a BadRequest
        /// result with error details.</returns>
        [HttpPut("change-password")]
        [Authorize]
        public async Task<ActionResult<AuthResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var response = await _accountService.ChangePasswordAsync(GetUserId()!, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Sends a password reset link to the email address specified in the request.
        /// </summary>
        /// <param name="request">The request containing the user's email address for which to send the reset password link. Cannot be null.</param>
        /// <returns>An <see cref="ActionResult{ApiResponse}"/> indicating the result of the operation. Returns a success
        /// response if the reset password email was sent; otherwise, returns a bad request response with an error
        /// message.</returns>
        [HttpPost("send-reset-password-link")]
        public async Task<ActionResult<ApiResponse>> SendResetPasswordLink([FromBody] ForgetPasswordRequest request)
        {
            try
            {
                bool result = await _passwordService.SendResetPasswordLinkAsync(request);
                if (!result)
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Failed to send reset password link."));

                return Ok(new ApiResponse(200, "Reset Password Email was sent to your email."));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Resets the user's password using the information provided in the request.
        /// </summary>
        /// <param name="request">The request containing the user's password reset information. Must not be null.</param>
        /// <returns>An <see cref="ActionResult{T}"/> containing a <see cref="AuthResponse"/> if the password reset is
        /// successful; otherwise, a bad request response with error details.</returns>
        [HttpPost("reset-password")]
        public async Task<ActionResult<AuthResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var response = await _passwordService.ResetPasswordAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Sends admin notification for new user registration
        /// </summary>
        private async Task NotifyAdminOfRegistrationAsync(AuthResponse userResponse)
        {
            try
            {
                if (userResponse == null)
                    return;

                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about new registration");
                    return;
                }

                // Determine user role from the role string in response
                var notificationType = userResponse.Role?.Equals("Trainer", StringComparison.OrdinalIgnoreCase) ?? false
                    ? NotificationType.NewTrainerRegistration
                    : NotificationType.NewClientRegistration;

                await _adminNotificationService.CreateAdminNotificationAsync(
                    adminUserId: admin.Id,
                    title: $"New {userResponse.Role} Registration",
                    message: $"{userResponse.Name} ({userResponse.Email}) has registered as a {userResponse.Role}",
                    type: notificationType,
                    relatedEntityId: userResponse.Id,
                    broadcastToAll: true
                );

                _logger.LogInformation("Admin notified of new {Role} registration: {Email}",
                    userResponse.Role, userResponse.Email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to notify admin of new registration");
                // Don't rethrow - registration already succeeded
            }
        }

        /// <summary>
        /// Sends admin notification for Google registration
        /// </summary>
        private async Task NotifyAdminOfGoogleRegistrationAsync(AuthResponse userResponse)
        {
            try
            {
                if (userResponse == null)
                    return;

                var admin = await _adminUserResolver.GetPrimaryAdminAsync();
                if (admin == null)
                {
                    _logger.LogWarning("No admin user found to notify about Google registration");
                    return;
                }

                var notificationType = userResponse.Role?.Equals("Trainer", StringComparison.OrdinalIgnoreCase) ?? false
                    ? NotificationType.NewTrainerRegistration
                    : NotificationType.NewClientRegistration;

                await _adminNotificationService.CreateAdminNotificationAsync(
                    adminUserId: admin.Id,
                    title: $"New {userResponse.Role} Registration (Google Auth)",
                    message: $"{userResponse.Name} ({userResponse.Email}) has registered as a {userResponse.Role} using Google authentication",
                    type: notificationType,
                    relatedEntityId: userResponse.Id,
                    broadcastToAll: true
                );

                _logger.LogInformation("Admin notified of new Google {Role} registration: {Email}",
                    userResponse.Role, userResponse.Email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to notify admin of Google registration");
                // Don't rethrow - registration already succeeded
            }
        }
        private string? GetUserId()
        {
            return User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}