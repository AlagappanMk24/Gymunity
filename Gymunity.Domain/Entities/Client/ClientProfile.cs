using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Entities.Trainer;

namespace Gymunity.Domain.Entities.Client
{
    public class ClientProfile : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public int? HeightCm { get; set; }
        public decimal? StartingWeightKg { get; set; }
        public string? Gender { get; set; }
        public string? Goal { get; set; } // "Fat Loss", "Muscle Gain", etc.
        public string? ExperienceLevel { get; set; } // Beginner, Intermediate, Advanced
        public bool IsOnboardingCompleted { get; set; } = false;
        public bool IsSuspended { get; set; }
        public DateTime SuspendedAt { get; set; }
        public ICollection<BodyStatLog>? BodyStatLogs { get; set; }
        public ICollection<WorkoutLog>? WorkoutLogs { get; set; }
        public AppUser User { get; set; } = null!;
        public ICollection<Subscription> Subscriptions { get; set; } = [];
        public ICollection<TrainerReview> TrainerReviews { get; set; } = new List<TrainerReview>();
    }
}