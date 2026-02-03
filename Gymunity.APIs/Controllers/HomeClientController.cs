using Gymunity.Application.Contracts.Services;
using Gymunity.Application.DTOs.Client;
using ITI.Gymunity.FP.Application.DTOs.Client;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.APIs.Controllers
{
    /// <summary>
    /// Controller responsible for providing read-only access to packages, trainers, and programs for clients and guests.
    /// Handles searching and browsing the marketplace.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HomeClientController(IHomeClientService homeService) : ControllerBase
    {
        private readonly IHomeClientService _homeService = homeService;

        /// <summary>
        /// Searches across packages, programs, and trainers using a single search term.
        /// </summary>
        /// <param name="term">The search keyword for names or descriptions.</param>
        /// <returns>A combined result containing matching packages, programs, and trainers.</returns>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Search([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest("Search term is required.");

            var allPackages = await _homeService.GetAllPackagesAsync();
            var matchingPackages = allPackages
                .Where(p => !string.IsNullOrEmpty(p.Name) && p.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || !string.IsNullOrEmpty(p.Description) && p.Description.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var (programs, trainers) = await _homeService.SearchAsync(term);

            return Ok(new { packages = matchingPackages, programs, trainers });
        }

        /// <summary>
        /// Retrieves all training packages available in the system.
        /// </summary>
        /// <returns>A list of available packages.</returns>
        [HttpGet("packages")]
        [ProducesResponseType(typeof(IEnumerable<PackageClientResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PackageClientResponse>>> GetAllPackages()
        {
            var packages = await _homeService.GetAllPackagesAsync();
            return Ok(packages);
        }

        /// <summary>
        /// Gets a specific package by its identifier.
        /// </summary>
        /// <param name="id">The numeric ID of the package.</param>
        /// <returns>The package details if found; otherwise, NotFound.</returns>
        [HttpGet("packages/{id:int}")]
        [ProducesResponseType(typeof(PackageClientResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PackageClientResponse>> GetPackageById(int id)
        {
            var pkg = await _homeService.GetPackageByIdAsync(id);
            if (pkg is null) return NotFound();
            return Ok(pkg);
        }

        // GET: api/client/homeclient/trainers
        /// <summary>
        /// Retrieves a list of all trainer profiles.
        /// </summary>
        [HttpGet("trainers")]
        [ProducesResponseType(typeof(IEnumerable<TrainerClientResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TrainerClientResponse>>> GetAllTrainers()
        {
            var trainers = await _homeService.GetAllTrainersAsync();
            return Ok(trainers);
        }

        // GET: api/homeclient/trainers/{id}
        /// <summary>
        /// Gets a trainer's profile details by their profile ID.
        /// </summary>
        [HttpGet("trainers/{id:int}")]
        [ProducesResponseType(typeof(TrainerClientResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TrainerClientResponse>> GetTrainerById(int id)
        {
            var trainer = await _homeService.GetTrainerByIdAsync(id);
            if (trainer is null) return NotFound();
            return Ok(trainer);
        }

        // GET: api/homeclient/trainers/{trainerProfileId}/packages
        /// <summary>
        /// Retrieves all packages offered by a specific trainer using their profile ID.
        /// </summary>
        [HttpGet("trainers/{trainer-profile-id:int}/packages")]
        [ProducesResponseType(typeof(IEnumerable<PackageClientResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PackageClientResponse>>> GetPackagesByTrainerProfileId(int trainerProfileId)
        {
            var packages = await _homeService.GetPackagesByTrainerIdAsync(trainerProfileId);
            return Ok(packages);
        }

        // GET: api/homeclient/packages/byTrainerUser/{trainerUserId}
        /// <summary>
        /// Retrieves all packages offered by a trainer using their User ID.
        /// </summary>
        [HttpGet("packages/by-trainer-user/{trainerUserId}")]
        [ProducesResponseType(typeof(IEnumerable<PackageClientResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PackageClientResponse>>> GetPackagesByTrainerUserId(int trainerUserId)
        {
            var packages = await _homeService.GetPackagesByTrainerAsync(trainerUserId);
            return Ok(packages);
        }

        //// GET: api/homeclient/packages/byTrainer/{trainerProfileId}
        //[HttpGet("packages/byTrainer/{trainerProfileId:int}")]
        //public async Task<ActionResult<IEnumerable<PackageClientResponse>>> GetPackagesByTrainerProfileIdAlt(int trainerProfileId)
        //{
        //    var packages = await _homeService.GetPackagesByTrainerIdAsync(trainerProfileId);
        //    return Ok(packages);
        //}

        // --- Client program endpoints requested ---
        // GET: api/homeclient/programs
        /// <summary>
        /// Retrieves all fitness programs available in the library.
        /// </summary>
        [HttpGet("programs")]
        [ProducesResponseType(typeof(IEnumerable<ProgramClientResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProgramClientResponse>>> GetAllPrograms()
        {
            var programs = await _homeService.GetAllProgramsAsync();
            return Ok(programs);
        }

        // GET: api/homeclient/programs/{id}
        /// <summary>
        /// Gets a specific fitness program by its identifier.
        /// </summary>
        [HttpGet("programs/{id:int}")]
        [ProducesResponseType(typeof(ProgramClientResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProgramClientResponse>> GetProgramById(int id)
        {
            var program = await _homeService.GetProgramByIdAsync(id);
            if (program is null) return NotFound();
            return Ok(program);
        }

        // GET: api/homeclient/programs/byTrainer/{trainerId} (trainerId is user id)
        /// <summary>
        /// Retrieves programs created by a trainer using their User ID.
        /// </summary>
        [HttpGet("programs/by-trainer/{trainerId}")]
        [ProducesResponseType(typeof(IEnumerable<ProgramClientResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProgramClientResponse>>> GetProgramsByTrainer(string trainerId)
        {
            var list = await _homeService.GetProgramsByTrainerIdAsync(trainerId);
            return Ok(list);
        }

        // New: GET: api/homeclient/programs/byTrainerUser/{trainerUserId} (user id)
        /// <summary>
        /// Retrieves a collection of fitness programs associated with a trainer's unique User ID.
        /// </summary>
        /// <param name="trainerUserId">The unique identity (string) of the trainer user.</param>
        /// <returns>A list of programs belonging to the specified trainer account.</returns>
        [HttpGet("programs/byTrainerUser/{trainerUserId}")]
        [ProducesResponseType(typeof(IEnumerable<ProgramClientResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProgramClientResponse>>> GetProgramsByTrainerUserId(string trainerUserId)
        {
            var list = await _homeService.GetProgramsByTrainerIdAsync(trainerUserId);
            return Ok(list);
        }

        // New: GET: api/homeclient/programs/byTrainerProfile/{trainerProfileId} (profile id -> maps to user id)
        /// <summary>
        /// Retrieves programs created by a trainer using their Profile ID.
        /// </summary>
        [HttpGet("programs/by-trainer-profile/{trainerProfileId:int}")]
        [ProducesResponseType(typeof(IEnumerable<ProgramClientResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProgramClientResponse>>> GetProgramsByTrainerProfileId(int trainerProfileId)
        {
            var list = await _homeService.GetProgramsByTrainerProfileIdAsync(trainerProfileId);
            return Ok(list);
        }
    }
}