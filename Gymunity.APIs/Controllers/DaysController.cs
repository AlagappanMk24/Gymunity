using Gymunity.APIs.Areas.Trainer;
using Gymunity.APIs.Responses;
using Gymunity.APIs.Responses.Errors;
using Gymunity.Application.Contracts.Services.Trainer;
using Gymunity.Application.DTOs.Program;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.APIs.Controllers
{
    /// <summary>
    /// Controller for managing specific days within a training week.
    /// </summary>
    public class DaysController(IDayService service) : TrainerBaseController
    {
        private readonly IDayService _service = service;

        /// <summary>
        /// Retrieves all training days associated with a specific program week.
        /// </summary>
        /// <param name="weekId">The numeric ID of the parent week.</param>
        /// <returns>A list of training days.</returns>
        [HttpGet("by-week/{weekId:int}")]
        [ProducesResponseType(typeof(IEnumerable<ProgramDayGetAllResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByWeek(int weekId)
        {
            var list = await _service.GetByWeekAsync(weekId);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves details for a specific training day by its ID.
        /// </summary>
        /// <param name="id">The numeric ID of the day.</param>
        /// <returns>The day details if found; otherwise, NotFound.</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProgramDayGetAllResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        /// <summary>
        /// Creates a new training day within a program week.
        /// </summary>
        /// <param name="request">The data required to create a training day.</param>
        /// <returns>The newly created day details.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProgramDayGetAllResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ProgramDayGetAllResponse request)
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates an existing training day's information.
        /// </summary>
        /// <param name="id">The ID of the day to update.</param>
        /// <param name="request">The updated day data.</param>
        /// <returns>NoContent if successful; otherwise, NotFound.</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] ProgramDayGetAllResponse request)
        {
            var ok = await _service.UpdateAsync(id, request);
            if (!ok) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Deletes a specific training day.
        /// </summary>
        /// <param name="id">The ID of the day to remove.</param>
        /// <returns>NoContent if successful; otherwise, NotFound.</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}