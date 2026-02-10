using Gymunity.APIs.Responses;
using Gymunity.APIs.Services;
using Gymunity.Application.Contracts.ExternalServices.Auth;
using Gymunity.Application.Contracts.Services.Identity;
using Gymunity.Application.DTOs.Account;
using Gymunity.Application.DTOs.Account.OTP;
using Gymunity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gymunity.APIs.Controllers
{
    // <summary>
    /// Provides API endpoints for authenticated user account management operations.
    /// </summary>
    /// <remarks>
    /// This controller handles operations related to user profiles and account settings.
    /// All endpoints require authentication and operate on the currently logged-in user.
    /// </remarks>
    /// <param name="accountService">The account service used for user profile management operations.</param>
    /// <param name="passwordService">The password service used for password change operations.</param>
    /// <param name="logger">The logger instance for recording controller operations and errors.</param>
    [Route("api/account")]
    public class AccountController(
        IAccountService accountService,
        IPasswordService passwordService,
        ILogger<AccountController> logger) : BaseApiController
    {
        private readonly IAccountService _accountService = accountService;
        private readonly IPasswordService _passwordService = passwordService;
        private readonly ILogger<AccountController> _logger = logger;

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
        private string? GetUserId()
        {
            return User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}