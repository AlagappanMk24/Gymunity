using Gymunity.Domain.Entities.ProgramAggregate;

namespace Gymunity.Domain.Entities.Client
{
    public class WorkoutLog : BaseEntity
    {
        public new long Id { get; set; } // long for high volume
        public int ClientProfileId { get; set; } 
        public ClientProfile ClientProfile { get; set; } = null!;
        public int ProgramDayId { get; set; }
        public ProgramDay ProgramDay { get; set; } = null!;
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
        public int? DurationMinutes { get; set; }
        public string ExercisesLoggedJson { get; set; } = "[]"; // full set/rep/weight log
    }
}