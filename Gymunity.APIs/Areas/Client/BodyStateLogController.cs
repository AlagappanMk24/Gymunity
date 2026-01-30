using Gymunity.APIs.Responses;
using Gymunity.APIs.Responses.Errors;
using Gymunity.Application.Contracts.Services.Client;
using Gymunity.Application.DTOs.ClientDto;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gymunity.APIs.Areas.Client
{
    /// <summary>
    /// Controller for tracking client body metrics (weight, body fat, etc.) over time.
    /// </summary>
    /// <param name="bodyStateLogService">The service used to manage body state log data.</param>
    public class BodyStateLogController(IBodyStateLogService bodyStateLogService) : ClientBaseController
    {
        private readonly IBodyStateLogService _bodyStateLogService = bodyStateLogService;

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Records a new body state entry for the authenticated client.
        /// </summary>
        /// <param name="request">The weight and body metric data to log.</param>
        /// <returns>The newly created body state log entry.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BodyStateLogResponse), StatusCodes.Status201Created)]
        public async Task<ActionResult> AddAsync([FromBody] CreateBodyStateLogRequest request)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized"));

            var result = await _bodyStateLogService.AddAsync(userId, request);
            return Created(nameof(GetLastStateLog), result);
        }


        /// <summary>
        /// Retrieves the full history of body state logs for the authenticated client.
        /// </summary>
        /// <returns>A list of historical body state entries.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<BodyStateLogResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<BodyStateLogResponse>>> GetAllStateLogsByClient()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized"));

            var result = await _bodyStateLogService.GetStateLogsByClientAsync(userId);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves the most recent body state entry recorded by the client.
        /// </summary>
        /// <returns>The latest body state log entry.</returns>
        [HttpGet("lastStateLog")]
        [ProducesResponseType(typeof(BodyStateLogResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public  async Task<ActionResult> GetLastStateLog()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized"));

            var result = await _bodyStateLogService.GetLastStateLog(userId);
            return Ok(result);
        }

    }
}
