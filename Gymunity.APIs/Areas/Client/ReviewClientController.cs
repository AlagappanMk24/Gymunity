using Gymunity.Application.Contracts.Services.Client;
using Gymunity.Application.DTOs.Trainers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gymunity.APIs.Areas.Client
{
    /// <summary>
    /// Controller allowing clients to manage reviews for trainers they have worked with.
    /// </summary>
    /// <param name="service">The service handling trainer review business logic.</param>
    public class ReviewClientController(IReviewClientService service) : ClientBaseController
    {
        private readonly IReviewClientService _service = service;

        private string GetUserId() {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        /// <summary>
        /// Creates a new review for a specific trainer.
        /// </summary>
        /// <param name="trainerId">The unique ID of the trainer being reviewed.</param>
        /// <param name="request">The review details including rating and comment.</param>
        /// <returns>The created review data.</returns>
        [HttpPost("trainer/{trainer-id:int}")]
        public async Task<IActionResult> CreateForTrainer(int trainerId, [FromBody] TrainerReviewCreateRequest request)
        {
            try
            {
                var created = await _service.CreateAsync(
                    GetUserId(),
                    trainerId,
                    request
                );

                return Ok(created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing review owned by the authenticated client.
        /// </summary>
        /// <param name="reviewId">The unique ID of the review to update.</param>
        /// <param name="request">The updated review content.</param>
        /// <returns>The updated review response.</returns>
        [HttpPut("{review-id:int}")]
        public async Task<IActionResult> Update(int reviewId, [FromBody] TrainerReviewCreateRequest request)
        {
            var updated = await _service.UpdateAsync(GetUserId(), reviewId, request);
            if (updated == null) return BadRequest(new { success = false, message = "Update failed or review not found/owned by user" });
            return Ok(updated);
        }

        /// <summary>
        /// Deletes a specific review owned by the authenticated client.
        /// </summary>
        /// <param name="reviewId">The unique ID of the review to remove.</param>
        /// <returns>A success status indicator.</returns>
        [HttpDelete("{review-id:int}")]
        public async Task<IActionResult> Delete(int reviewId)
        {
            var deleted = await _service.DeleteAsync(GetUserId(), reviewId);
            if (!deleted) return BadRequest(new { success = false, message = "Delete failed or review not found/owned by user" });
            return Ok(new { success = true });
        }
    }
}