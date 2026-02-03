using Gymunity.Application.Contracts.Services.Trainer;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.APIs.Areas.Trainer
{
    /// <summary>
    /// Controller for managing and retrieving reviews for trainer profiles.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ReviewTrainerController"/> class.
    /// </remarks>
    /// <param name="service">The review service for business logic and moderation.</param>
    public class ReviewTrainerController(IReviewTrainerService service) : TrainerBaseController
    {
        private readonly IReviewTrainerService _service = service;

        /// <summary>
        /// Retrieves all approved reviews for a specific trainer to display on their public profile.
        /// </summary>
        /// <param name="trainerId">The unique identifier of the trainer profile.</param>
        /// <returns>A list of verified and approved reviews.</returns>
        ///     // GET: api/trainer/reviews/byTrainer/{trainerId}
        // Guest can view approved reviews for any trainer
        [HttpGet("by-trainer/{trainerId:int}")]
        public async Task<IActionResult> GetApprovedReviewsForTrainer(int trainerId)
        {
            var list = await _service.GetApprovedForTrainerAsync(trainerId);
            return Ok(list);
        }
    }
}

/*
    Standard Controller Design Approach:

    - This controller does NOT handle any authorization, identity, or role checks.
    - The controller's responsibility is limited to:
        � Receiving HTTP requests
        � Passing data directly to the Application Service
        � Returning the service result as an HTTP response

    - Authentication / Authorization concerns are centralized in:
        � Base controllers (e.g., ClientBaseController, TrainerBaseController)
        � Global filters / middleware (JWT, Policies, Roles)

    - This ensures:
        � Clean Controllers (Thin Controllers pattern)
        � Single Responsibility Principle
        � Consistent behavior across all controllers
        � Easy switch between Anonymous / Authorized modes without rewriting controllers

    - Any future authorization logic should be applied at:
        � Base controller level
        � Attribute level ([Authorize], [AllowAnonymous])
        � NOT inside action methods
*/

//��� ���� ���� ���� ���� ��� �� Action:

// NOTE:
// Authorization is handled at the BaseController / middleware level.
// This action assumes a valid execution context and delegates all logic to the service layer.