using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gymunity.APIs.Areas.Trainer
{
    /// <summary>
    /// Base controller for trainer-related API endpoints, providing shared configuration and helper methods.
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Trainer")]
    [Route("api/v1/trainers/[controller]")]
    public class TrainerBaseController : ControllerBase
    {
        /// <summary>
        /// Extracts the Trainer's unique identifier from the current user's claims.
        /// </summary>
        /// <returns>A string representing the user's NameIdentifier claim.</returns>
        protected string? GetTrainerId()
        {
            // Try to read trainer id from claims (NameIdentifier expected to be trainer profile id or user id)
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
          
        }
    }
}