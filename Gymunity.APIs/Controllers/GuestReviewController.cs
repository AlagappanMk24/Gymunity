using Gymunity.Application.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.APIs.Controllers
{
    /// <summary>
    /// Controller that provides public (guest) access to trainer reviews and leaderboards.
    /// </summary>
    /// <remarks>
    /// This controller does not require authentication, allowing prospective clients 
    /// to see trainer social proof before signing up.
    /// </remarks>
    [Route("api/guest/reviews")]
    [ApiController]
    public class GuestReviewController(IGuestReviewService service) : ControllerBase
    {
        private readonly IGuestReviewService _service = service;

        /// <summary>
        /// Retrieves a list of approved reviews for a specific trainer profile.
        /// </summary>
        /// <param name="profileId">The unique identifier of the trainer's profile.</param>
        /// <returns>A collection of approved review DTOs.</returns>
        [HttpGet("by-trainer/{profileId:int}")]
        public async Task<IActionResult> GetByTrainer(int profileId)
        {
            var res = await _service.GetApprovedReviewsByTrainerAsync(profileId);
            return Ok(res);
        }

        /// <summary>
        /// Retrieves the top-rated trainers based on client feedback and engagement.
        /// </summary>
        /// <returns>A list of highly-ranked trainer profiles for the landing page.</returns>
        [HttpGet("top-trainers")]
        public async Task<IActionResult> GetTopTrainers()
        {
            var res = await _service.GetTopTrainersAsync();
            return Ok(res);
        }
    }
}