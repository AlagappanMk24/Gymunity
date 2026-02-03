using Gymunity.APIs.Responses;
using Gymunity.Application.Contracts.Services.Packages;
using Gymunity.Application.DTOs.Packages;
using Gymunity.Application.DTOs.Trainers;
using Gymunity.Domain;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Interfaces.Trainer;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.APIs.Areas.Trainer
{
    /// <summary>
    /// Controller for managing training packages, subscriptions, and profit analytics.
    /// </summary>
    [ApiController]
    [Route("api/trainer/[controller]")]
    public class PackagesController(IPackageService service, IUnitOfWork unitOfWork) : TrainerBaseController
    {
        private readonly IPackageService _service = service;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        /// <summary>
        /// Retrieves all available training packages.
        /// </summary>
        /// <returns>A list of all packages.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PackageResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        /// <summary>
        /// Retrieves a specific package by its identifier.
        /// </summary>
        /// <param name="id">The numeric ID of the package.</param>
        /// <returns>The package details if found.</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PackageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var package = await _service.GetByIdAsync(id);
            if (package == null)
                return NotFound();

            return Ok(package);
        }

        /// <summary>
        /// Retrieves all packages belonging to a specific trainer profile.
        /// </summary>
        /// <param name="trainerId">The numeric ID of the trainer profile.</param>
        /// <returns>A list of packages for the specified trainer.</returns>
        [HttpGet("by-trainer/{trainerId}")]
        [ProducesResponseType(typeof(IEnumerable<PackageResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByTrainer(int trainerId)
        {
            var list = await _service.GetAllForTrainerAsync(trainerId);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves packages for the current authenticated trainer along with subscription and client data.
        /// </summary>
        /// <returns>Package data with nested subscription details.</returns>
        [HttpGet("with-subscriptions")]
        [ProducesResponseType(typeof(IEnumerable<PackageWithSubscriptionsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllWithSubscriptions()
        {
            var currentUserId = GetTrainerId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Unauthorized(new { message = "Unauthorized" });

            var trainerRepo = _unitOfWork.Repository<TrainerProfile, ITrainerProfileRepository>();
            var allProfiles = await trainerRepo.GetAllAsync();
            var profile = allProfiles.FirstOrDefault(p => p.UserId == currentUserId && !p.IsDeleted);
            if (profile == null)
                return NotFound(new { message = "Trainer profile not found for current user." });

            var data = await _service.GetPackagesWithSubscriptionsForTrainerAsync(profile.Id);
            return Ok(data);
        }

        /// <summary>
        /// Creates a new training package with associated fitness programs.
        /// </summary>
        /// <param name="request">The package creation data (V2 supports program name resolution).</param>
        /// <returns>The newly created package.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(PackageResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] PackageCreateRequestV2 request)
        {
            try
            {
                // Use the V2 creation method so ProgramNames from the request are resolved to program ids
                var created = await _service.CreateAsyncV2(request.TrainerProfileId, request);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = created.Id },
                    created
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse(400, ex.Message));
            }
        }

        /// <summary>
        /// Updates an existing training package. This operation is idempotent.
        /// </summary>
        /// <param name="id">The ID of the package to update.</param>
        /// <param name="request">The updated package data.</param>
        /// <returns>The updated package details.</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(PackageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] PackageCreateRequestV2 request)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, new PackageCreateRequest
                {
                    Name = request.Name,
                    Description = request.Description,
                    PriceMonthly = request.PriceMonthly,
                    PriceYearly = request.PriceYearly,
                    IsActive = request.IsActive,
                    ThumbnailUrl = request.ThumbnailUrl,
                    ProgramIds = request.ProgramIds,
                    IsAnnual = request.IsAnnual,
                    PromoCode = request.PromoCode,
                    TrainerId = request.TrainerProfileId
                });

                if (!updated)
                    return NotFound();

                var pkg = await _service.GetByIdAsync(id);
                if (pkg == null)
                    return NoContent();

                // return full package after update
                return Ok(pkg);
            }
            catch (InvalidOperationException)
            {
                // On conflict, return current package as success to make endpoint idempotent
                var pkg = await _service.GetByIdAsync(id);
                if (pkg == null)
                    return NoContent();

                return Ok(pkg);
            }
        }

        /// <summary>
        /// Deletes a training package from the system.
        /// </summary>
        /// <param name="id">The ID of the package to remove.</param>
        /// <returns>NoContent if successful.</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <=0)
            {
                return BadRequest(new { message = "Invalid package id. Id must be a positive integer." });
            }

            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound(new { message = $"Package with id {id} not found." });

            return NoContent();
        }
        /// <summary>
        /// Toggles the active status of a package.
        /// </summary>
        /// <param name="id">The ID of the package to toggle.</param>
        /// <returns>NoContent if successful.</returns>
        [HttpPatch("toggle-active/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var toggled = await _service.ToggleActiveAsync(id);

            if (!toggled)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Retrieves profit analytics for the current trainer's packages.
        /// </summary>
        /// <returns>A summary of profit per package and total earnings.</returns>
        [HttpGet("with-profit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyPackagesWithProfit()
        {
            // Get current trainer user id from base controller
            var currentUserId = GetTrainerId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Unauthorized(new { message = "Unauthorized" });

            var trainerRepo = _unitOfWork.Repository<TrainerProfile, ITrainerProfileRepository>();
            var allProfiles = await trainerRepo.GetAllAsync();
            var profile = allProfiles.FirstOrDefault(p => p.UserId == currentUserId && !p.IsDeleted);
            if (profile == null)
                return NotFound(new { message = "Trainer profile not found for current user." });

            var data = await _service.GetPackagesWithProfitForTrainerAsync(profile.Id);
            var total = data.Sum(p => p.Profit);

            return Ok(new { TrainerProfileId = profile.Id, TrainerUserId = currentUserId, Packages = data, TotalProfit = total });
        }
    }
}


//?? ������� ���� ����
//?? ��� �� ������ ��� Guest�

//? ����:

//User

//Claims

//Unauthorized()

//[Authorize]

//?? ��� ������ ��� Parameters �����

//?? ������ ����� (����)

//���� ��� �� Temporary / Development Only
//�� ������� ���� ����:

//Guest:

//GET

//Trainer:

//POST / PUT / DELETE

//������ ��:

//[Authorize(Roles = "Trainer")]
