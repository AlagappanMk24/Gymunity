namespace Gymunity.Application.DTOs.ClientDto
{
    public class WorkoutLogResponse
    {
        public long Id { get; set; } // Include the Id in response
        public int ClientProfileId { get; set; }
        public int ProgramDayId { get; set; }
        public string ProgramDayName { get; set; } = string.Empty; // e.g., "Day 1: Chest & Triceps"
        public DateTime CompletedAt { get; set; }
        public string? Notes { get; set; }
        public int? DurationMinutes { get; set; }
        public string ExercisesLoggedJson { get; set; } = "[]";
        public DateTimeOffset CreatedAt { get; set; } // From BaseEntity
        public DateTimeOffset? UpdatedAt { get; set; } // From BaseEntity
    }
}