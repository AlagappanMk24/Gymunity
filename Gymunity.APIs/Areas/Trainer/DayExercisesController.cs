using Gymunity.APIs.Responses;
using Gymunity.APIs.Responses.Errors;
using Gymunity.Application.Contracts.Services.Trainer;
using Gymunity.Application.DTOs.Program;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.APIs.Areas.Trainer
{
    /// <summary>
    /// Controller for managing specific exercises assigned to a training day.
    /// </summary>
    public class DayExercisesController(IDayExerciseService service) : TrainerBaseController
    {
        private readonly IDayExerciseService _service = service;

        /// <summary>
        /// Retrieves all exercises assigned to a specific training day.
        /// </summary>
        /// <param name="dayId">The numeric ID of the parent training day.</param>
        /// <returns>A list of exercises for that day.</returns>
        [HttpGet("by-day/{dayId:int}")]
        [ProducesResponseType(typeof(IEnumerable<ProgramDayExerciseGetAllResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByDay(int dayId)
        {
            var list = await _service.GetByDayAsync(dayId);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves a specific exercise assignment by its ID.
        /// </summary>
        /// <param name="id">The numeric ID of the exercise assignment.</param>
        /// <returns>The exercise details if found.</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProgramDayExerciseGetAllResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        /// <summary>
        /// Assigns an exercise to a training day.
        /// </summary>
        /// <param name="request">The data required to create the exercise assignment.</param>
        /// <returns>The newly created exercise assignment details.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProgramDayExerciseGetAllResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ProgramDayExerciseGetAllResponse request)
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates the details (sets, reps, rest) of an assigned exercise.
        /// </summary>
        /// <param name="id">The ID of the exercise assignment to update.</param>
        /// <param name="request">The updated assignment data.</param>
        /// <returns>NoContent if successful.</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] ProgramDayExerciseGetAllResponse request)
        {
            var ok = await _service.UpdateAsync(id, request);
            if (!ok) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Removes an exercise from a training day.
        /// </summary>
        /// <param name="id">The ID of the assignment to remove.</param>
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