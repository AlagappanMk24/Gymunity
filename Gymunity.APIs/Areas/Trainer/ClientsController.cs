using Gymunity.Application.Contracts.Services.Trainer;
using Gymunity.Application.DTOs.Client;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.APIs.Areas.Trainer
{
    /// <summary>
    /// Controller for managing and retrieving clients assigned to a specific trainer.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ClientsController"/> class.
    /// </remarks>
    /// <param name="service">The client service for business logic.</param>
    public class ClientsController(IClientService service) : TrainerBaseController
    {
        private readonly IClientService _service = service;

        /// <summary>
        /// Retrieves a list of all active clients associated with a specific trainer.
        /// </summary>
        /// <param name="trainerId">The unique identifier of the trainer.</param>
        /// <returns>A collection of client profiles including subscription status.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TrainerClientResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllClients(string trainerId)
        {
            var list = await _service.GetAllByTrainerIdAsync(trainerId);
            return Ok(list);
        }
    }
}