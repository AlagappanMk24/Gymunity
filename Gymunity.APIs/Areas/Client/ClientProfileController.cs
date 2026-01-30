using Gymunity.APIs.Responses;
using Gymunity.APIs.Responses.Errors;
using Gymunity.Application.Contracts.Services.Client;
using Gymunity.Application.DTOs.ClientDto;
using Gymunity.Domain.Entities.Client;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gymunity.APIs.Areas.Client
{
    /// <summary>
    /// Controller for managing client-specific profile data and dashboard metrics.
    /// </summary>   
    public class ClientProfileController(IClientProfileService clientProfileService, ILogger<ClientProfile> logger) : ClientBaseController
    {
        private readonly IClientProfileService _clientProfileService = clientProfileService;
        private readonly ILogger _logger = logger;

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Retrieves the dashboard statistics and overview for the authenticated client.
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ClientProfileDashboardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClientProfileDashboardResponse>> GetDashboard()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized"));

            try
            {
                var dashboard = await _clientProfileService.GetDashboardAsync(userId);
                return Ok(dashboard);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Dashboard not found for UserId: {UserId}", userId);
                return NotFound(new ApiResponse(404, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard for UserId: {UserId}", userId);
                return StatusCode(500, new ApiResponse(500, "An error occurred while retrieving dashboard"));
            }
        }

        /// <summary>
        /// Retrieves the profile details of the currently authenticated client.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ClientProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClientProfileResponse>> GetMyProfile()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized"));

            var profile = await _clientProfileService.GetClientProfileAsync(userId);

            if (profile == null)
                return NotFound(new ApiResponse(404, "Client Profile not found"));

            return Ok(profile);
        }

        /// <summary>
        /// Creates a new profile for the authenticated user.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ClientProfileResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult> CreateClientProfile(ClientProfileRequest request)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized"));

            var result = await _clientProfileService.CreateClientProfileAsync(userId, request);

            if (result == null)
                return Conflict(new ApiResponse(409, "Client profile already exists"));

            return Created(nameof(GetMyProfile), result);
        }

        /// <summary>
        /// Updates the existing profile information for the authenticated client.
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(ClientProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClientProfileResponse>> UpdateMyProfile([FromBody] ClientProfileRequest request)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized"));

            var updatedProfile = await _clientProfileService.UpdateClientProfileAsync(userId, request);

            if (updatedProfile == null)
                return NotFound(new ApiResponse(404, "Profile not found."));

            return Ok(updatedProfile);
        }


        /// <summary>
        /// Deletes the authenticated client's profile from the system.
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DeleteProfile()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized"));

            var result = await _clientProfileService.DeleteProfileAsync(userId);

            if (!result)
                return NotFound(new ApiResponse(404, "Profile not found"));

            return NoContent();
        }
    }
}