using Gymunity.Application.Contracts.Services.Trainer;
using Gymunity.Application.DTOs.Program;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.APIs.Areas.Trainer
{
    /// <summary>
    /// Controller for managing training program days within the trainer portal.
    /// </summary>
    /// <param name="service">The service handling day-related business logic.</param>
    public class DaysController(IDayService service) : TrainerBaseController
    {
        private readonly IDayService _service = service;

        /// <summary>
        /// Retrieves all training days associated with a specific program week.
        /// </summary>
        /// <param name="weekId">The unique identifier of the week.</param>
        /// <returns>A list of program days for the specified week.</returns>
        [HttpGet("by-week/{weekId:int}")]
        public async Task<IActionResult> GetByWeek(int weekId)
        {
            var list = await _service.GetByWeekAsync(weekId);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves a specific training day by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the program day.</param>
        /// <returns>The program day details if found; otherwise, NotFound.</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        /// <summary>
        /// Creates a new training day entry in a program.
        /// </summary>
        /// <param name="request">The data transfer object containing the day details.</param>
        /// <returns>The newly created day with a link to its location.</returns>
        [HttpPost]
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
        /// <returns>NoContent if successful; NotFound if the day doesn't exist.</returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProgramDayGetAllResponse request)
        {
            var ok = await _service.UpdateAsync(id, request);
            if (!ok) return NotFound();
            return NoContent();
        }
        /// <summary>
        /// Deletes a specific training day from the program.
        /// </summary>
        /// <param name="id">The ID of the day to remove.</param>
        /// <returns>NoContent if successful; NotFound if the day doesn't exist.</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}