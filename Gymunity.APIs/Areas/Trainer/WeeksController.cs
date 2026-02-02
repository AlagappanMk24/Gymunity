using Gymunity.APIs.Responses;
using Gymunity.APIs.Responses.Errors;
using Gymunity.Application.Contracts.Services.Trainer;
using Gymunity.Application.DTOs.Program;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.APIs.Areas.Trainer
{
    /// <summary>
    /// Controller for managing the weekly structure of fitness programs.
    /// </summary>
    public class WeeksController(IWeekService service) : TrainerBaseController
    {
        private readonly IWeekService _service = service;

        /// <summary>
        /// Retrieves all weeks associated with a specific fitness program.
        /// </summary>
        /// <param name="programId">The numeric ID of the parent program.</param>
        /// <returns>A list of weeks belonging to the program.</returns>
        [HttpGet("by-program/{programId:int}")]
        [ProducesResponseType(typeof(IEnumerable<ProgramWeekGetAllResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByProgram(int programId)
        {
            var list = await _service.GetByProgramAsync(programId);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves a specific program week by its ID.
        /// </summary>
        /// <param name="id">The numeric ID of the week.</param>
        /// <returns>The week details if found.</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProgramWeekGetAllResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        /// <summary>
        /// Adds a new week to a fitness program.
        /// </summary>
        /// <param name="request">The data required to create a program week.</param>
        /// <returns>The newly created week details.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProgramWeekGetAllResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ProgramWeekGetAllResponse request)
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates the details of an existing program week.
        /// </summary>
        /// <param name="id">The ID of the week to update.</param>
        /// <param name="request">The updated week data.</param>
        /// <returns>NoContent if successful.</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] ProgramWeekGetAllResponse request)
        {
            var ok = await _service.UpdateAsync(id, request);
            if (!ok) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Deletes a specific week from a program.
        /// </summary>
        /// <param name="id">The ID of the week to remove.</param>
        /// <returns>NoContent if successful.</returns>
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