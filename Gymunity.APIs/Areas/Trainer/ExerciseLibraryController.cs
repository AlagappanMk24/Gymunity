using Gymunity.APIs.Responses;
using Gymunity.APIs.Responses.Errors;
using Gymunity.Application.Contracts.Services.Trainer;
using Gymunity.Application.DTOs.ExerciseLibrary;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.APIs.Areas.Trainer
{
    /// <summary>
    /// Controller for managing the library of reusable exercises available to trainers.
    /// </summary>
    public class ExerciseLibraryController(IExerciseLibraryService service) : TrainerBaseController
    {
        private readonly IExerciseLibraryService _service = service;

        /// <summary>
        /// Retrieves all exercises, optionally filtered by a specific trainer.
        /// </summary>
        /// <param name="trainerId">The optional ID of the trainer to filter private exercises.</param>
        /// <returns>A list of exercises from the library.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ExerciseGetAllResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] string? trainerId = null)
        {
            var list = await _service.GetAllAsync(trainerId);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves a specific library exercise by its unique identifier.
        /// </summary>
        /// <param name="id">The numeric ID of the exercise.</param>
        /// <returns>The exercise details if found; otherwise, NotFound.</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ExerciseGetAllResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        /// <summary>
        /// Searches for exercises in the library by name.
        /// </summary>
        /// <param name="name">The name or keyword to search for.</param>
        /// <param name="trainerId">Optional trainer ID to include their private exercises in results.</param>
        /// <returns>A filtered list of exercises.</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<ExerciseGetAllResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] string name, [FromQuery] string? trainerId = null)
        {
            var list = await _service.SearchByNameAsync(name, trainerId);
            return Ok(list);
        }

        /// <summary>
        /// Adds a new exercise to the library.
        /// </summary>
        /// <param name="request">The data required to create a library exercise.</param>
        /// <returns>The newly created exercise details.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ExerciseGetAllResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ExerciseCreateRequest request)
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates an existing exercise in the library.
        /// </summary>
        /// <param name="id">The ID of the exercise to update.</param>
        /// <param name="request">The updated exercise data.</param>
        /// <returns>NoContent if successful.</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] ExerciseUpdateRequest request)
        {
            var ok = await _service.UpdateAsync(id, request);
            if (!ok) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Removes an exercise from the library.
        /// </summary>
        /// <param name="id">The ID of the exercise to delete.</param>
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