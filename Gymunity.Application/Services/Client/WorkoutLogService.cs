using AutoMapper;
using Gymunity.Application.Contracts.Services.Client;
using Gymunity.Application.DTOs.ClientDto;
using Gymunity.Application.Specifications.Client;
using Gymunity.Domain;
using Gymunity.Domain.Entities;
using Gymunity.Domain.Entities.Client;
using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Enums;
using Gymunity.Domain.Interfaces.Client;
using Microsoft.Extensions.Logging;

namespace Gymunity.Application.Services.Client
{
    public class WorkoutLogService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<WorkoutLog> logger) : IWorkoutLogService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<WorkoutLog> _logger = logger;

        public async Task<WorkoutLogResponse> AddWorkoutLogAsync(string userId, WorkoutLogRequest request) 
        {
            var specs = new ClientWithUserSpecs(c => c.UserId == userId);

            var profile = await _unitOfWork.Repository<ClientProfile, IClientProfileRepository>()
                .GetWithSpecsAsync(specs);

            if (profile == null)
            {
                _logger.LogWarning("Client profile not found for UserId: {UserId}", userId);
                throw new InvalidOperationException("Client profile not found");
            }

            var programDay = await _unitOfWork.Repository<ProgramDay>().GetByIdAsync(request.ProgramDayId);

            if (programDay == null)
            {
                _logger.LogWarning("ProgramDay {ProgramDayId} not found", request.ProgramDayId);
                throw new InvalidOperationException($"Program day with ID {request.ProgramDayId} not found");
            }


            // Verify client has access to this program via active subscription

            var subscriptionSpecs = new SubscriptionsWithClientAndProgramSpecs(s => s.ClientId == userId &&
            s.Status == SubscriptionStatus.Active);

            var hasAccess = await _unitOfWork.Repository<Subscription>()
                .GetAllWithSpecsAsync(subscriptionSpecs);

            if (!hasAccess.Any())
            {
                _logger.LogWarning("Client {UserId} does not have an active subscription", userId);
                throw new InvalidOperationException("Client does not have an active subscription");
            }

            var workoutLog = _mapper.Map<WorkoutLog>(request);
            workoutLog.ClientProfileId = profile.Id;

            //ValidateExercisesJson(workoutLog.ExercisesLoggedJson);

            _unitOfWork.Repository<WorkoutLog>().Add(workoutLog);

            try
            {
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("WorkoutLog created successfully for ClientId: {ClientId}, ProgramDayId: {ProgramDayId}",
                    profile.Id, request.ProgramDayId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving WorkoutLog for ClientId: {ClientId}, ProgramDayId: {ProgramDayId}",
                    profile.Id, request.ProgramDayId);
                throw;
            }

            return _mapper.Map<WorkoutLogResponse>(workoutLog);
        }

        public async Task<WorkoutLogResponse?> GetWorkoutLogByIdAsync(string userId, long workoutLogId)
        {
            var profileSpecs = new ClientWithUserSpecs(c => c.UserId == userId);

            var profile = await _unitOfWork.Repository<ClientProfile>().GetWithSpecsAsync(profileSpecs);
            if (profile == null)
            {
                _logger.LogWarning("Client profile not found for UserId: {UserId}", userId);
                throw new InvalidOperationException("Client Profile not found.");
            }

            var workoutLogSpecs = new WorkoutLogWithDetailsSpecs(w => w.Id == workoutLogId &&
            w.ClientProfileId == profile.Id);

            var workoutLog = await _unitOfWork.Repository<WorkoutLog>().GetWithSpecsAsync(workoutLogSpecs);

            return workoutLog == null ? null : _mapper.Map<WorkoutLogResponse>(workoutLog);
        }

        public async Task<IEnumerable<WorkoutLogResponse>> GetWorkoutLogsByClientAsync(string userId, int? pageNumber = null, int? pageSize = null)
        {
            var profileSpecs = new ClientWithUserSpecs(c => c.UserId == userId);

            var profile = await _unitOfWork.Repository<ClientProfile>().GetWithSpecsAsync(profileSpecs);
            if (profile == null)
            {
                _logger.LogWarning("Client profile not found for UserId: {UserId}", userId);
                throw new InvalidOperationException("Client Profile not found.");
            }

            WorkoutLogsByClientSpecs workoutLogSpecs;

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                workoutLogSpecs = new WorkoutLogsByClientSpecs(profile.Id, pageNumber.Value, pageSize.Value);
            }
            else
            {
                workoutLogSpecs = new WorkoutLogsByClientSpecs(profile.Id);
            }
            var workoutLogs = await _unitOfWork.Repository<WorkoutLog>()
            .GetAllWithSpecsAsync(workoutLogSpecs);

            return _mapper.Map<IEnumerable<WorkoutLogResponse>>(workoutLogs);
        }

        public async Task<WorkoutLogResponse> UpdateWorkoutLogAsync(string userId, long workoutLogId ,WorkoutLogRequest request)
        {
            var profileSpecs = new ClientWithUserSpecs(c => c.UserId == userId);

            var profile = await _unitOfWork.Repository<ClientProfile>().GetWithSpecsAsync(profileSpecs);
            if (profile == null)
            {
                _logger.LogWarning("Client profile not found for UserId: {UserId}", userId);
                throw new InvalidOperationException("Client Profile not found.");
            }

            var workoutLogSpecs = new WorkoutLogWithDetailsSpecs(w => w.ClientProfileId == profile.Id &&
            w.Id == workoutLogId);

            var workoutLog = await _unitOfWork.Repository<WorkoutLog>().GetWithSpecsAsync(workoutLogSpecs);

            if (workoutLog == null)
            {
                _logger.LogWarning("WorkoutLog {WorkoutLogId} not found for ClientId: {ClientId}",
                    workoutLogId, profile.Id);
                throw new InvalidOperationException("Workout log not found");
            }

            if (request.ProgramDayId != workoutLog.ProgramDayId)
            {
                var programDay = await _unitOfWork.Repository<ProgramDay>().GetByIdAsync(request.ProgramDayId);
                if (programDay == null)
                {
                    if (programDay == null)
                        throw new InvalidOperationException($"Program day with ID {request.ProgramDayId} not found");
                }
            }

                _mapper.Map(request, workoutLog);
                //ValidateExercisesJson(workoutLog.ExercisesLoggedJson);

                 _unitOfWork.Repository<WorkoutLog>().Update(workoutLog);

                try
                {
                    await _unitOfWork.CompleteAsync();
                    _logger.LogInformation("WorkoutLog {WorkoutLogId} updated successfully", workoutLogId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating WorkoutLog {WorkoutLogId}", workoutLogId);
                    throw;
                }

                return _mapper.Map<WorkoutLogResponse>(workoutLog);
        }

        public async Task<bool> DeleteWorkoutLogAsync(string userId, long workoutLogId)
        {
            var specs = new ClientWithUserSpecs(c => c.UserId == userId);
            var profile = await _unitOfWork.Repository<ClientProfile, IClientProfileRepository>()
                .GetWithSpecsAsync(specs);

            if (profile == null)
                throw new InvalidOperationException("Client profile not found");

            var workoutLog = await _unitOfWork.Repository<WorkoutLog, IWorkoutLogRepository>()
                .GetByIdAsync(workoutLogId);

            if (workoutLog == null || workoutLog.ClientProfileId != profile.Id)
            {
                _logger.LogWarning("WorkoutLog {WorkoutLogId} not found or unauthorized for ClientId: {ClientId}",
                    workoutLogId, profile.Id);
                return false;
            }

            _unitOfWork.Repository<WorkoutLog>().Delete(workoutLog);

            try
            {
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("WorkoutLog {WorkoutLogId} deleted successfully", workoutLogId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting WorkoutLog {WorkoutLogId}", workoutLogId);
                throw;
            }
        }

        //private void ValidateExercisesJson(string json)
        //{
        //    try
        //    {
        //        System.Text.Json.JsonDocument.Parse(json);
        //    }
        //    catch (System.Text.Json.JsonException ex)
        //    {
        //        _logger.LogWarning(ex, "Invalid JSON format in ExercisesLoggedJson");
        //        throw new ArgumentException("Exercise log contains invalid JSON format", nameof(json));
        //    }
        //}
    }
}
