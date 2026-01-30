using Gymunity.APIs.Responses;
using Gymunity.Application.Contracts.Services.Client;
using Gymunity.Application.DTOs.ClientDto;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gymunity.APIs.Areas.Client
{
    /// <summary>
    /// Handles the initial onboarding process for new clients.
    /// </summary>
    /// <param name="onboardingService">Service to handle onboarding business logic.</param>
    public class OnboardingController(IOnboardingService onboardingService) : ClientBaseController
    {
        private readonly IOnboardingService _onboardingService = onboardingService;

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Completes the profile onboarding for the authenticated client.
        /// </summary>
        /// <param name="request">The onboarding data including goals and physical metrics.</param>
        /// <returns>A success message or an error if the profile is already completed.</returns>
        [HttpPut("complete")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> CompleteProfileOnboardingAsync(OnboardingRequest request)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized"));

            try
            {
                var result = await _onboardingService.CompleteOnboardingAsync(userId, request);

                if (!result)
                    return Conflict(new ApiResponse(409, "Profile already completed"));

                return Ok(new ApiResponse(200, "Your profile is completed"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse(400, ex.Message));
            }
        }

        /// <summary>
        /// Checks if the authenticated client has already finished the onboarding process.
        /// </summary>
        /// <returns>True if onboarding is complete; otherwise, false.</returns>
        [HttpGet("status")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<bool>> IsProfileOnboardingCompleted()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized"));

            var isCompleted = await _onboardingService.IsProfileOnboardingCompletedAsync(userId);

            return Ok(isCompleted);
        }
    }
}