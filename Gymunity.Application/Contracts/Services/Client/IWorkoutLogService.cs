using Gymunity.Application.DTOs.ClientDto;

namespace Gymunity.Application.Contracts.Services.Client
{
    public interface IWorkoutLogService
    {
        Task<WorkoutLogResponse> AddWorkoutLogAsync(string userId, WorkoutLogRequest request);
        Task<WorkoutLogResponse?> GetWorkoutLogByIdAsync(string userId, long workoutLogId);
        Task<IEnumerable<WorkoutLogResponse>> GetWorkoutLogsByClientAsync(string userId, int? pageNumber = null, int? pageSize = null);
        Task<WorkoutLogResponse> UpdateWorkoutLogAsync(string userId, long workoutLogId, WorkoutLogRequest request);
        Task<bool> DeleteWorkoutLogAsync(string userId, long workoutLogId);
    }
}
