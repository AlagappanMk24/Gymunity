using Gymunity.APIs.Responses;
using Gymunity.APIs.Responses.Errors;
using Gymunity.Application.Contracts.Services.Client;
using Gymunity.Application.DTOs.ClientDto;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gymunity.APIs.Areas.Client
{
    /// <summary>
    /// Controller for managing client workout logs, including creating, retrieving, updating, and deleting logs.
    /// </summary>
    /// <param name="workoutLogService">The service handling workout log business logic.</param>
    public class WorkoutLogController(IWorkoutLogService workoutLogService) : ClientBaseController
    {
        private readonly IWorkoutLogService _workoutLogService = workoutLogService;
        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Creates a new workout log for the authenticated user.
        /// </summary>
        /// <param name="request">The workout log details.</param>
        /// <returns>The created workout log.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(WorkoutLogResponse), StatusCodes.Status201Created)]
        public async Task<ActionResult> AddAsync([FromBody] WorkoutLogRequest request)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized"));

            try
            {
                var result = await _workoutLogService.AddWorkoutLogAsync(userId, request);

                return Created(nameof(GetWorkoutLogById), request);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse(400, ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(500, "An error occurred while creating workout log"));
            }
        }

        /// <summary>
        /// Retrieves a specific workout log by its unique identifier.
        /// </summary>
        /// <param name="id">The ID of the workout log.</param>
        /// <returns>The requested workout log.</returns>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(WorkoutLogResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetWorkoutLogById(long id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized"));

            var workoutLog = await _workoutLogService.GetWorkoutLogByIdAsync(userId, id);

            if (workoutLog == null)
                return NotFound(new ApiResponse(404, "WorkoutLog not found"));

            return Ok(workoutLog);
        }

        /// <summary>
        /// Retrieves a paginated list of workout logs for the authenticated user.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A collection of workout logs.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<WorkoutLogResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetWorkoutLogs([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized"));


            var workoutLogs = await _workoutLogService.GetWorkoutLogsByClientAsync(userId, pageNumber, pageSize);
            return Ok(workoutLogs);
        }

        /// <summary>
        /// Updates an existing workout log.
        /// </summary>
        /// <param name="id">The ID of the log to update.</param>
        /// <param name="request">The updated workout log data.</param>
        /// <returns>The updated workout log response.</returns>
        [HttpPut("{id:long}")]
        [ProducesResponseType(typeof(WorkoutLogResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateWorkoutLogAsync(long id, [FromBody] WorkoutLogRequest request)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized"));

            try
            {
                var response = await _workoutLogService.UpdateWorkoutLogAsync(userId, id, request);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse(404, ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(500, "An error occurred while updating workout log"));
            }
        }

        /// <summary>
        /// Deletes a specific workout log from the system.
        /// </summary>
        /// <param name="id">The ID of the workout log to delete.</param>
        /// <returns>No content on success, otherwise not found.</returns>
        [HttpDelete("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteWorkoutLog(long id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(401, "Unauthorized"));

            var result = await _workoutLogService.DeleteWorkoutLogAsync(userId, id);

            if (!result)
                return NotFound(new ApiResponse(404, "Workout log not found"));

            return NoContent();
        }
    }
}