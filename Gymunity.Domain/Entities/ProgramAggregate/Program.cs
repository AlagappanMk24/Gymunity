using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Enums;

namespace Gymunity.Domain.Entities.ProgramAggregate
{
    public class Program : BaseEntity
    {
        // Legacy DB compatibility: keep TrainerId column (string) because database currently expects it.
        // Prefer using TrainerProfileId as the new canonical reference.
        public string? TrainerId { get; set; } = string.Empty; // legacy, filled from TrainerProfile.UserId on create/update

        // Use TrainerProfileId as the reference to the trainer profile
        public int? TrainerProfileId { get; set; }
        public TrainerProfile? TrainerProfile { get; set; }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public ProgramType Type { get; set; } // Workout, Nutrition, Hybrid, Challenge
        public int DurationWeeks { get; set; }
        public decimal? Price { get; set; } // null = only via subscription
        public bool IsPublic { get; set; } = true;
        public int? MaxClients { get; set; }
        public string? ThumbnailUrl { get; set; }
        public ICollection<ProgramWeek> Weeks { get; set; } = new List<ProgramWeek>();
        public ICollection<PackageProgram> PackagePrograms { get; set; } = new List<PackageProgram>();

        // Optional relation to TrainerProfile stored above
    }
}
