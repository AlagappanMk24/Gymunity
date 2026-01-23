using AutoMapper;
using Gymunity.Application.Contracts.Services.Admin;
using Gymunity.Application.DTOs.Program;
using Gymunity.Application.Specifications.Admin;
using Gymunity.Domain;
using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gymunity.Application.Services.Admin
{
    /// <summary>
    /// Admin service for managing programs, reviews, and program lifecycle
    /// </summary>
    public class ProgramAdminService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProgramAdminService> logger) : IProgramAdminService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<ProgramAdminService> _logger = logger;
        private readonly IProgramRepository _programRepo = unitOfWork.Repository<Program, IProgramRepository>();

        /// <summary>
        /// Get all programs with optional filtering and pagination
        /// </summary>
        public async Task<IEnumerable<ProgramGetAllResponse>> GetAllProgramsAsync(
            ProgramFilterSpecs specs)
        {
            try
            {
                var programs = await _programRepo.GetAllWithSpecsAsync(specs);
                return _mapper.Map<IEnumerable<ProgramGetAllResponse>>(programs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving programs with specs");
                throw;
            }
        }

        /// <summary>
        /// Get program by ID with full details including trainer and weeks
        /// </summary>
        public async Task<ProgramGetByIdResponse?> GetProgramByIdAsync(int programId)
        {
            try
            {
                var program = await _programRepo.GetByIdWithIncludesAsync(programId);
                if (program == null)
                {
                    _logger.LogWarning("Program with ID {ProgramId} not found", programId);
                    return null;
                }

                var response = _mapper.Map<ProgramGetByIdResponse>(program);

                _logger.LogInformation("Retrieved program details for {ProgramId}", programId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving program details for ID {ProgramId}", programId);
                throw;
            }
        }

        /// <summary>
        /// Get detailed program information for admin view using specification pattern
        /// Includes trainer profile with user data and program weeks
        /// </summary>
        public async Task<ProgramGetByIdResponse?> GetProgramDetailsWithTrainerAsync(int programId)
        {
            try
            {
                // Use specification pattern to load program with trainer data
                var spec = new ProgramDetailWithTrainerSpecs(programId);
                var programs = await _programRepo.GetAllWithSpecsAsync(spec);
                var program = programs.FirstOrDefault();

                if (program == null)
                {
                    _logger.LogWarning("Program with ID {ProgramId} not found", programId);
                    return null;
                }

                var response = _mapper.Map<ProgramGetByIdResponse>(program);

                _logger.LogInformation("Retrieved program details with trainer data for {ProgramId}", programId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving program details with trainer for ID {ProgramId}", programId);
                throw;
            }
        }

        /// <summary>
        /// Get count of programs matching specification
        /// </summary>
        public async Task<int> GetProgramCountAsync(ProgramFilterSpecs specs)
        {
            try
            {
                return await _programRepo.GetCountWithspecsAsync(specs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting program count");
                throw;
            }
        }

        /// <summary>
        /// Get public programs
        /// </summary>
        public async Task<IEnumerable<ProgramGetAllResponse>> GetPublicProgramsAsync(
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var specs = new ProgramFilterSpecs(
                    isPublic: true,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                var programs = await _programRepo.GetAllWithSpecsAsync(specs);
                return _mapper.Map<IEnumerable<ProgramGetAllResponse>>(programs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public programs");
                throw;
            }
        }

        /// <summary>
        /// Get private programs
        /// </summary>
        public async Task<IEnumerable<ProgramGetAllResponse>> GetPrivateProgramsAsync(
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var specs = new ProgramFilterSpecs(
                    isPublic: false,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                var programs = await _programRepo.GetAllWithSpecsAsync(specs);
                return _mapper.Map<IEnumerable<ProgramGetAllResponse>>(programs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving private programs");
                throw;
            }
        }

        /// <summary>
        /// Get programs by trainer
        /// </summary>
        public async Task<IEnumerable<ProgramGetAllResponse>> GetProgramsByTrainerAsync(
            int trainerProfileId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var programs = await _programRepo.GetByTrainerAsyncProfileId(trainerProfileId);
                var paginated = programs
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return _mapper.Map<IEnumerable<ProgramGetAllResponse>>(paginated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving programs for trainer {TrainerId}", trainerProfileId);
                throw;
            }
        }

        /// <summary>
        /// Search programs by title or description
        /// </summary>
        public async Task<IEnumerable<ProgramGetAllResponse>> SearchProgramsAsync(
            string searchTerm,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var programs = await _programRepo.SearchAsync(searchTerm);
                var paginated = programs
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return _mapper.Map<IEnumerable<ProgramGetAllResponse>>(paginated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching programs with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        /// <summary>
        /// Get total count of public programs
        /// </summary>
        public async Task<int> GetPublicProgramCountAsync()
        {
            try
            {
                var specs = new ProgramFilterSpecs(isPublic: true);
                return await _programRepo.GetCountWithspecsAsync(specs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public program count");
                throw;
            }
        }

        /// <summary>
        /// Get total count of private programs
        /// </summary>
        public async Task<int> GetPrivateProgramCountAsync()
        {
            try
            {
                var specs = new ProgramFilterSpecs(isPublic: false);
                return await _programRepo.GetCountWithspecsAsync(specs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting private program count");
                throw;
            }
        }

        /// <summary>
        /// Get programs count by trainer
        /// </summary>
        public async Task<int> GetTrainerProgramCountAsync(int trainerProfileId)
        {
            try
            {
                var programs = await _programRepo.GetByTrainerAsyncProfileId(trainerProfileId);
                return programs.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting program count for trainer {TrainerId}", trainerProfileId);
                throw;
            }
        }

        /// <summary>
        /// Update program details (admin only)
        /// </summary>
        public async Task<bool> UpdateProgramAsync(int programId, ProgramUpdateRequest request)
        {
            try
            {
                var program = await _programRepo.GetByIdAsync(programId);
                if (program == null)
                {
                    _logger.LogWarning("Program with ID {ProgramId} not found for update", programId);
                    return false;
                }

                program.Title = request.Title;
                program.Description = request.Description;
                program.Type = request.Type;
                program.DurationWeeks = request.DurationWeeks;
                program.Price = request.Price;
                program.IsPublic = request.IsPublic;
                program.MaxClients = request.MaxClients;
                program.ThumbnailUrl = request.ThumbnailUrl;
                program.UpdatedAt = DateTime.UtcNow;

                _programRepo.Update(program);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Program {ProgramId} updated successfully", programId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating program {ProgramId}", programId);
                throw;
            }
        }

        /// <summary>
        /// Delete a program (soft or hard delete)
        /// </summary>
        public async Task<bool> DeleteProgramAsync(int programId, bool softDelete = true)
        {
            try
            {
                var program = await _programRepo.GetByIdAsync(programId);
                if (program == null)
                {
                    _logger.LogWarning("Program with ID {ProgramId} not found for deletion", programId);
                    return false;
                }

                if (softDelete)
                {
                    program.IsDeleted = true;
                    program.UpdatedAt = DateTime.UtcNow;
                    _programRepo.Update(program);
                }
                else
                {
                    _programRepo.Delete(program);
                }

                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Program {ProgramId} deleted (soft: {SoftDelete})", programId, softDelete);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting program {ProgramId}", programId);
                throw;
            }
        }

        /// <summary>
        /// Toggle program visibility (public/private)
        /// </summary>
        public async Task<bool> ToggleProgramVisibilityAsync(int programId)
        {
            try
            {
                var program = await _programRepo.GetByIdAsync(programId);
                if (program == null)
                {
                    _logger.LogWarning("Program with ID {ProgramId} not found", programId);
                    return false;
                }

                program.IsPublic = !program.IsPublic;
                program.UpdatedAt = DateTime.UtcNow;

                _programRepo.Update(program);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Program {ProgramId} visibility toggled to {IsPublic}", programId, program.IsPublic);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling visibility for program {ProgramId}", programId);
                throw;
            }
        }

        /// <summary>
        /// Get program stats (usage, reviews, subscriptions)
        /// </summary>
        public async Task<ProgramStatsDto> GetProgramStatsAsync(int programId)
        {
            try
            {
                var program = await _programRepo.GetByIdAsync(programId);
                if (program == null)
                {
                    _logger.LogWarning("Program with ID {ProgramId} not found for stats", programId);
                    throw new InvalidOperationException($"Program with ID {programId} not found");
                }

                var weeksCount = await _programRepo.GetWeeksCount(programId);
                var exercisesCount = await _programRepo.GetTotalExercisesCount(programId);

                var stats = new ProgramStatsDto
                {
                    ProgramId = programId,
                    Title = program.Title,
                    TrainerName = program.TrainerProfile?.User?.FullName ?? "Unknown",
                    TotalWeeks = weeksCount,
                    TotalExercises = exercisesCount,
                    CreatedAt = program.CreatedAt,
                    UpdatedAt = program.UpdatedAt ?? DateTime.UtcNow,
                    IsPublic = program.IsPublic,
                    Price = program.Price,
                    MaxClients = program.MaxClients
                };

                _logger.LogInformation("Retrieved stats for program {ProgramId}", programId);
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving program stats for ID {ProgramId}", programId);
                throw;
            }
        }

        /// <summary>
        /// Get programs statistics summary
        /// </summary>
        public async Task<ProgramsSummaryDto> GetProgramsSummaryAsync()
        {
            try
            {
                var allPrograms = await _programRepo.GetAllAsync();
                var publicPrograms = allPrograms.Where(p => p.IsPublic).ToList();
                var privatePrograms = allPrograms.Where(p => !p.IsPublic).ToList();

                return new ProgramsSummaryDto
                {
                    TotalPrograms = allPrograms.Count(),
                    PublicPrograms = publicPrograms.Count,
                    PrivatePrograms = privatePrograms.Count,
                    AverageDurationWeeks = allPrograms.Any() ? allPrograms.Average(p => p.DurationWeeks) : 0,
                    ProgramsWithPrice = allPrograms.Count(p => p.Price.HasValue),
                    TotalTrainers = allPrograms.Select(p => p.TrainerProfileId).Distinct().Count()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving programs summary");
                throw;
            }
        }
    }

    /// <summary>
    /// Program statistics DTO
    /// </summary>
    public class ProgramStatsDto
    {
        public int ProgramId { get; set; }
        public string Title { get; set; } = null!;
        public string TrainerName { get; set; } = null!;
        public int TotalWeeks { get; set; }
        public int TotalExercises { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public bool IsPublic { get; set; }
        public decimal? Price { get; set; }
        public int? MaxClients { get; set; }
    }

    /// <summary>
    /// Programs summary DTO
    /// </summary>
    public class ProgramsSummaryDto
    {
        public int TotalPrograms { get; set; }
        public int PublicPrograms { get; set; }
        public int PrivatePrograms { get; set; }
        public double AverageDurationWeeks { get; set; }
        public int ProgramsWithPrice { get; set; }
        public int TotalTrainers { get; set; }
    }
}
