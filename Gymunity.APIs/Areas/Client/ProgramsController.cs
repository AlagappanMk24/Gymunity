using Gymunity.APIs.Responses;
using Gymunity.Application.Contracts.Services.Client;
using Gymunity.Application.DTOs.Program;
using Gymunity.Application.DTOs.Program.ProgramDayDtos;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gymunity.APIs.Areas.Client
{
    /// <summary>
    /// Provides API endpoints for retrieving and managing client-specific program data for the authenticated user.
    /// </summary>
    /// <remarks>This controller requires the user to be authenticated. All actions operate in the context of
    /// the currently logged-in user and return appropriate HTTP responses for unauthorized access or invalid
    /// requests.</remarks>
    /// <param name="programsService">The service used to access and manage client program information.</param>
    public class ProgramsController(IClientProgramsService programsService) : ClientBaseController
    {
        private readonly IClientProgramsService programsService = programsService;
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        /// <summary>
        /// Retrieves the list of active programs associated with the currently authenticated user.
        /// </summary>
        /// <returns>An <see cref="ActionResult{T}"/> containing a collection of <see cref="ProgramResponse"/> objects
        /// representing the user's active programs. Returns an unauthorized response if the user is not authenticated,
        /// or a bad request response if an error occurs.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProgramResponse>>> GetActivePrograms()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized));
            try
            {
                var programs = await programsService.GetUserActiveProgramsAsync(userId);
                return Ok(programs);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, ex.Message));
            }
        }


        /// <summary>
        /// Retrieves the details of a specific program by its unique identifier.
        /// </summary>
        /// <remarks>This endpoint requires the caller to be an authenticated user. If the user is not
        /// authorized to access the requested program, an unauthorized response is returned.</remarks>
        /// <param name="programId">The unique identifier of the program to retrieve.</param>
        /// <returns>An <see cref="ActionResult{ProgramResponse}"/> containing the program details if found; otherwise, an
        /// appropriate error response such as 401 Unauthorized or 400 Bad Request.</returns>
        [HttpGet("{program-id}")]
        public async Task<ActionResult<ProgramResponse>> GetProgramById(int programId)
        {
            try
            {
                var program = await programsService.GetProgramByIdAsync(GetCurrentUserId(), programId);
                return Ok(program);
            }
            catch (UnauthorizedAccessException uaEx)
            {
                return Unauthorized(new ApiResponse(401, uaEx.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, ex.Message));
            }
        }

        /// <summary>
        /// Retrieves all weeks associated with the specified program.
        /// </summary>
        /// <param name="programId">The unique identifier of the program for which to retrieve weeks.</param>
        /// <returns>An <see cref="ActionResult{T}"/> containing a collection of <see cref="ProgramWeekResponse"/> objects
        /// representing the weeks in the specified program. Returns 401 Unauthorized if the user is not authenticated
        /// or does not have access to the program. Returns 400 Bad Request if an error occurs.</returns>
        [HttpGet("{program-id}/weeks")]
        public async Task<ActionResult<IEnumerable<ProgramWeekResponse>>> GetProgramWeeks(int programId)
        {
            try
            {
                var weeks = await programsService.GetAllWeeks(GetCurrentUserId(), programId);
                return Ok(weeks);
            }
            catch (UnauthorizedAccessException uaEx)
            {
                return Unauthorized(new ApiResponse(401, uaEx.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, ex.Message));
            }
        }

        /// <summary>
        /// Retrieves all program days associated with the specified week.
        /// </summary>
        /// <param name="weekId">The unique identifier of the week for which to retrieve program days.</param>
        /// <returns>An <see cref="ActionResult{T}"/> containing a collection of <see cref="ProgramDayResponse"/> objects for the
        /// specified week. Returns 401 Unauthorized if the user is not authenticated, or 400 Bad Request if an error
        /// occurs.</returns>
        [HttpGet("weeks/{week-id}/days")]
        public async Task<ActionResult<IEnumerable<ProgramDayResponse>>> GetWeekDays(int weekId)
        {
            try
            {
                var days = await programsService.GetAllDays(GetCurrentUserId(), weekId);
                return Ok(days);
            }
            catch (UnauthorizedAccessException uaEx)
            {
                return Unauthorized(new ApiResponse(401, uaEx.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, ex.Message));
            }

        }

        /// <summary>
        /// Retrieves the details of a specific program day by its unique identifier.
        /// </summary>
        /// <param name="dayId">The unique identifier of the program day to retrieve.</param>
        /// <returns>An <see cref="ActionResult{ProgramDayResponse}"/> containing the program day details if found; otherwise, an
        /// appropriate error response such as 401 Unauthorized or 400 Bad Request.</returns>
        [HttpGet("days/{day-id}")]
        public async Task<ActionResult<ProgramDayResponse>> GetDayById(int dayId)
        {
            try
            {
                var day = await programsService.GetDayByIdAsync(GetCurrentUserId(), dayId);
                return Ok(day);
            }
            catch (UnauthorizedAccessException uaEx)
            {
                return Unauthorized(new ApiResponse(401, uaEx.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, ex.Message));
            }
        }
    }
}
