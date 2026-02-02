using AutoMapper;
using Gymunity.Application.Contracts.Services.Client;
using Gymunity.Application.DTOs.Program;
using Gymunity.Application.DTOs.Program.ProgramDayDtos;
using Gymunity.Application.Specifications.Client;
using Gymunity.Application.Specifications.ProgramSpecs;
using Gymunity.Domain;
using Gymunity.Domain.Entities;
using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Interfaces.Trainer;
using Microsoft.Extensions.Logging;

namespace Gymunity.Application.Services.Client
{
    public class ClientProgramsService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ClientProgramsService> logger) : IClientProgramsService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<ClientProgramsService> _logger = logger;
        private readonly IProgramRepository _programRepo = unitOfWork.Repository<Program, IProgramRepository>();


        /// <summary>
        /// Gets all active programs for the user by their subscriptions.
        /// Uses sequential execution to avoid DbContext concurrent operation errors.
        /// </summary>
        public async Task<IEnumerable<ProgramResponse>> GetUserActiveProgramsAsync(string userId)
        {
            // FIX: Load subscriptions with programs first
            var subscribtionsSpecs = new ActiveSubscriptionsWithProgramsByUserIdSpecification(userId);
            var subscriptions = await _unitOfWork.Repository<Subscription>().GetAllWithSpecsAsync(subscribtionsSpecs);

            if (!subscriptions.Any())
            {
                _logger.LogInformation("No active programs found for user with ID {UserId}", userId);
                return [];
            }

            var activePrograms = new List<ProgramResponse>();

            // FIX: Sequential execution instead of concurrent Task.WhenAll()
            // This prevents "second operation was started on this context" error
            foreach (var packageProgram in subscriptions.SelectMany(s => s.Package.PackagePrograms))
            {
                try
                {
                    var response = await MapProgramToResponseAsync(packageProgram.Program);
                    activePrograms.Add(response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error mapping program {ProgramId} for user {UserId}",
                        packageProgram.Program.Id, userId);
                }
            }

            return [.. activePrograms.DistinctBy(p => p.Id)];
        }

        /// <summary>
        /// Gets a specific program by ID after verifying user access.
        /// </summary>
        public async Task<ProgramResponse?> GetProgramByIdAsync(string userId, int programId)
        {
            // FIX: Check access first with all needed data
            var (hasAccess, _) = await UserHasAccessToProgramWithSubscriptionsAsync(userId, programId);

            if (!hasAccess)
            {
                _logger.LogWarning("User does not have access to program with ID {ProgramId}", programId);
                throw new UnauthorizedAccessException("You do not have access to this program.");
            }

            var program = await _programRepo.GetByIdAsync(programId);
            if (program == null)
            {
                _logger.LogWarning("Program with ID {ProgramId} not found", programId);
                throw new Exception("Program not found.");
            }

            // FIX: Map separately after all DbContext operations complete
            return await MapProgramToResponseAsync(program);
        }

        /// <summary>
        /// Gets all weeks for a program after verifying user access.
        /// </summary>
        public async Task<IEnumerable<ProgramWeekResponse>> GetAllWeeks(string userId, int programId)
        {
            // FIX: Check access first
            var (hasAccess, _) = await UserHasAccessToProgramWithSubscriptionsAsync(userId, programId);

            if (!hasAccess)
            {
                _logger.LogWarning("User does not have access to program with ID {ProgramId}", programId);
                throw new UnauthorizedAccessException("You do not have access to this program.");
            }

            // FIX: Now fetch weeks in a separate DbContext operation
            var weeksSpecs = new ProgramWeeksByProgramIdSpecification(programId);
            var weeks = await _unitOfWork.Repository<ProgramWeek>().GetAllWithSpecsAsync(weeksSpecs);

            var weekResponses = weeks.Select(w => _mapper.Map<ProgramWeekResponse>(w)).ToList();
            return weekResponses;
        }

        /// <summary>
        /// Gets all days for a week after verifying user access.
        /// </summary>
        public async Task<IEnumerable<ProgramDayResponse>> GetAllDays(string userId, int weekId)
        {
            // FIX: Load week and program ID first
            var week = await _unitOfWork.Repository<ProgramWeek>().GetByIdAsync(weekId);
            if (week == null)
            {
                _logger.LogWarning("Week with ID {WeekId} not found", weekId);
                throw new Exception("Week not found.");
            }

            // FIX: Check access using combined method
            var (hasAccess, _) = await UserHasAccessToProgramWithSubscriptionsAsync(userId, week.ProgramId);
            if (!hasAccess)
            {
                _logger.LogWarning("User does not have access to program with ID {ProgramId}", week.ProgramId);
                throw new UnauthorizedAccessException("You do not have access to this program.");
            }

            // FIX: Now fetch days in separate DbContext operation
            var daysSpecs = new ProgramDaySpec(weekId);
            var days = await _unitOfWork.Repository<ProgramDay>().GetAllWithSpecsAsync(daysSpecs);

            var dayResponses = days.Select(d => _mapper.Map<ProgramDayResponse>(d)).ToList();

            return dayResponses;
        }

        /// <summary>
        /// Gets a specific day with all its exercises after verifying user access.
        /// </summary>
        public async Task<ProgramDayResponse?> GetDayByIdAsync(string userId, int dayId)
        {
            // FIX: Load day with exercises using specification
            var daySpecs = new ProgramDaySpec(d => d.Id == dayId);
            var day = await _unitOfWork.Repository<ProgramDay>().GetWithSpecsAsync(daySpecs);

            if (day == null)
            {
                _logger.LogWarning("Day with ID {DayId} not found", dayId);
                throw new Exception("Day not found.");
            }

            // FIX: Load week in separate operation after day operations complete
            var week = await _unitOfWork.Repository<ProgramWeek>().GetByIdAsync(day.ProgramWeekId);
            if (week == null)
            {
                _logger.LogWarning("Week with ID {WeekId} not found for Day ID {DayId}", day.ProgramWeekId, dayId);
                throw new Exception("Week not found.");
            }

            // FIX: Check access using combined method
            var (hasAccess, _) = await UserHasAccessToProgramWithSubscriptionsAsync(userId, week.ProgramId);
            if (!hasAccess)
            {
                _logger.LogWarning("User does not have access to program with ID {ProgramId}", week.ProgramId);
                throw new UnauthorizedAccessException("You do not have access to this program.");
            }

            // FIX: Map data - exercises are already loaded by specification
            var dayResponse = _mapper.Map<ProgramDayResponse>(day);
            dayResponse.Exercises = [.. day.Exercises.Select(e => _mapper.Map<ProgramDayExerciseResponse>(e))];

            return dayResponse;
        }

        /// <summary>
        /// OPTIMIZED: Checks user access and returns subscriptions to avoid duplicate queries.
        /// Replaces multiple calls to UserHasAccessToProgramAsync.
        /// </summary>
        private async Task<(bool hasAccess, Subscription[] subscriptions)>
            UserHasAccessToProgramWithSubscriptionsAsync(string userId, int programId)
        {
            var subscribtionsSpecs = new ActiveSubscriptionsWithProgramsByUserIdSpecification(userId);
            var subscriptions = await _unitOfWork.Repository<Subscription>().GetAllWithSpecsAsync(subscribtionsSpecs);

            var hasAccess = subscriptions
                .SelectMany(s => s.Package.PackagePrograms)
                .Any(pp => pp.ProgramId == programId);

            return (hasAccess, subscriptions.ToArray());
        }

        /// <summary>
        /// Maps program entity to response DTO with additional calculated fields.
        /// Performs separate DbContext operations for counts.
        /// </summary>
        private async Task<ProgramResponse> MapProgramToResponseAsync(Program program)
        {
            var response = _mapper.Map<ProgramResponse>(program);

            // FIX: These DbContext operations are called sequentially (not concurrently)
            // Each completes before the next begins, preventing concurrency errors
            response.DurationWeeks = await _programRepo.GetWeeksCount(program.Id);
            response.TotalExercises = await _programRepo.GetTotalExercisesCount(program.Id);

            return response;
        }
    }
}
