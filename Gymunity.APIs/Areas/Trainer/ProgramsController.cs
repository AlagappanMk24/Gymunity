using Gymunity.APIs.Responses;
using Gymunity.APIs.Responses.Errors;
using Gymunity.Application.Contracts.Services.Trainer;
using Gymunity.Application.DTOs.Program;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.APIs.Areas.Trainer
{
    /// <summary>
    /// Controller for managing fitness programs within the Trainer area.
    /// </summary>
    public class ProgramsController(ITrainerProgramService service) : TrainerBaseController
    {
        private readonly ITrainerProgramService _service = service;

        /// <summary>
        /// Retrieves a list of all available fitness programs.
        /// </summary>
        /// <returns>A collection of program summaries.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProgramResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        /// <summary>
        /// Searches for programs based on a specific search term.
        /// </summary>
        /// <param name="term">The keyword to search for in program titles or descriptions.</param>
        /// <returns>A filtered list of programs.</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<ProgramResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] string? term)
        {
            var list = await _service.SearchAsync(term);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves all programs created by a specific trainer.
        /// </summary>
        /// <param name="trainerId">The unique identifier of the trainer.</param>
        /// <returns>A list of programs associated with the trainer.</returns>
        [HttpGet("by-trainer/{trainer-id}")]
        [ProducesResponseType(typeof(IEnumerable<ProgramResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByTrainer(string trainerId)
        {
            var list = await _service.GetByTrainerAsync(trainerId);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves detailed information for a specific program by its ID.
        /// </summary>
        /// <param name="id">The numeric ID of the program.</param>
        /// <returns>The program details if found; otherwise, NotFound.</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProgramResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        /// <summary>
        /// Creates a new fitness program.
        /// </summary>
        /// <param name="request">The data required to create the program.</param>
        /// <returns>The newly created program details.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProgramResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ProgramCreateRequest request)
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates an existing fitness program.
        /// </summary>
        /// <param name="id">The ID of the program to update.</param>
        /// <param name="request">The updated program data.</param>
        /// <returns>NoContent if successful; otherwise, NotFound.</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] ProgramUpdateRequest request)
        {
            var ok = await _service.UpdateAsync(id, request);
            if (!ok) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Deletes a fitness program from the system.
        /// </summary>
        /// <param name="id">The ID of the program to remove.</param>
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