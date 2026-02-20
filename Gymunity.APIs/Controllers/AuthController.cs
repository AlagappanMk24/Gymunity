using Gymunity.APIs.Responses;
using Gymunity.APIs.Services;
using Gymunity.Application.Contracts.ExternalServices.Auth;
using Gymunity.Application.Contracts.Services.Identity;
using Gymunity.Application.DTOs.Account.OTP;
using Gymunity.Application.DTOs.Auth;
using Gymunity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.APIs.Controllers
{
    /// <summary>
    /// Provides API endpoints for user authentication operations.
    /// </summary>
    /// <remarks>
    /// This controller handles all authentication-related operations including registration,
    /// login, OTP verification, and Google authentication. All endpoints are accessible anonymously.
    /// </remarks>
    /// <param name="identityService">The identity service used for user registration and login operations.</param>
    /// <param name="googleAuthService">The external authentication service used for Google authentication.</param>
    /// <param name="adminNotificationService">The admin notification service used to notify administrators of new registrations.</param>
    /// <param name="adminUserResolver">The service used to resolve administrator users for notifications.</param>
    /// <param name="logger">The logger instance for recording controller operations and errors.</param>
    [Route("api/auth")]
    [ApiController]
    public class AuthController(
    IIdentityService identityService,
    IGoogleAuthService googleAuthService,
    IAdminNotificationService adminNotificationService,
    AdminUserResolverService adminUserResolver,
    ILogger<AuthController> logger) : BaseApiController
    {
        private readonly IIdentityService _identityService = identityService;
        private readonly IGoogleAuthService _googleAuthService = googleAuthService;
        private readonly IAdminNotificationService _adminNotificationService = adminNotificationService;
        private readonly AdminUserResolverService _adminUserResolver = adminUserResolver;
        private readonly ILogger<AuthController> _logger = logger;

        // ================= REGISTRATION ENDPOINTS =================

        /// <summary>
        /// Step 1: Initiate user registration - validates data and sends OTP
        /// </summary>
        /// <remarks>
        /// This endpoint validates user input, checks uniqueness, and sends OTP to email.
        /// Registration data is stored server-side for 10 minutes.
        /// </remarks>
        [HttpPost("register/initiate")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> InitiateRegistration([FromForm] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Initiating registration for {Email}", request.Email);

                var response = await _identityService.InitiateRegistrationAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration initiation failed for {Email}", request.Email);
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Step 2: Complete registration with OTP verification
        /// </summary>
        /// <remarks>
        /// This endpoint verifies OTP and creates the user account.
        /// Registration data must be initiated within the last 10 minutes.
        /// </remarks>
        [HttpPost("register/complete")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> CompleteRegistration([FromBody] CompleteRegistrationRequest request)
        {
            try
            {
                _logger.LogInformation("Completing registration for {Email} with OTP", request.Email);

                if (string.IsNullOrEmpty(request.OtpCode))
                    return BadRequest(new ApiResponse(400, "OTP code is required"));

                var response = await _identityService.CompleteRegistrationAsync(request.Email, request.OtpCode);

                await NotifyAdminOfRegistrationAsync(response);

                _logger.LogInformation("Registration completed successfully for {Email}", request.Email);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration completion failed for {Email}", request.Email);
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Resend OTP for registration verification
        /// </summary>
        /// <remarks>
        /// Resends OTP and extends registration session by 10 minutes.
        /// </remarks>
        [HttpPost("register/resend-otp")]
        [AllowAnonymous]
        public async Task<ActionResult<OtpResponse>> ResendRegistrationOtp([FromBody] SendOtpRequest request)
        {
            try
            {
                _logger.LogInformation("Resending registration OTP to {Email}", request.Email);

                var result = await _identityService.ResendRegistrationOtpAsync(request.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend registration OTP to {Email}", request.Email);
                return BadRequest(new ApiResponse(400, ex.Message));
            }
        }

        /// <summary>
        /// Verify OTP for registration (without completing registration)
        /// </summary>
        /// <remarks>
        /// Useful for frontend to verify OTP before allowing user to proceed.
        /// </remarks>
        [HttpPost("register/verify-otp")]
        [AllowAnonymous]
        public ActionResult<OtpResponse> VerifyRegistrationOtp([FromBody] VerifyOtpRequest request)
        {
            try
            {
                _logger.LogInformation("Verifying registration OTP for {Email}", request.Email);

                var result = _identityService.VerifyRegistrationOtp(request.Email, request.OtpCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify registration OTP for {Email}", request.Email);
                return BadRequest(new ApiResponse(400, ex.Message));
            }
        }

        // ================= LOGIN ENDPOINTS =================

        /// <summary>
        /// Authenticates user with optional OTP
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login attempt for {EmailOrUserName}", request.EmailOrUserName);

                var response = await _identityService.LoginAsync(request);

                if (response.RequiresOtp)
                {
                    _logger.LogInformation("OTP required for login of {Email}", response.Email);
                    return Ok(response);
                }

                _logger.LogInformation("Login successful for {Email}", response.Email);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {EmailOrUserName}", request.EmailOrUserName);
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Sends OTP for login verification
        /// </summary>
        [HttpPost("login/send-otp")]
        [AllowAnonymous]
        public async Task<ActionResult<OtpResponse>> SendLoginOtp([FromBody] SendOtpRequest request)
        {
            try
            {
                _logger.LogInformation("Sending login OTP to {Email}", request.Email);

                var result = await _identityService.SendLoginOtpAsync(request.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send login OTP to {Email}", request.Email);
                return BadRequest(new ApiResponse(400, ex.Message));
            }
        }

        /// <summary>
        /// Completes login with OTP verification
        /// </summary>
        [HttpPost("login/complete")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> CompleteLogin([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Completing login with OTP for {EmailOrUserName}", request.EmailOrUserName);

                if (string.IsNullOrEmpty(request.OtpCode))
                    return BadRequest(new ApiResponse(400, "OTP code is required"));

                var response = await _identityService.LoginAsync(request);

                if (!response.RequiresOtp && !string.IsNullOrEmpty(response.Token))
                {
                    _logger.LogInformation("Login completed successfully for {Email}", response.Email);
                    return Ok(response);
                }

                return BadRequest(new ApiResponse(400, "OTP verification failed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login completion failed for {EmailOrUserName}", request.EmailOrUserName);
                return BadRequest(new ApiResponse(400, ex.Message));
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

                // Determine user role from the role 
                var notificationType = userResponse.Role == UserRole.Trainer
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

                var notificationType = userResponse.Role == UserRole.Trainer
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
    }
}