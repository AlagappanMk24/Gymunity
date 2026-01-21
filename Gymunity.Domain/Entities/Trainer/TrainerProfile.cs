using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Entities.Messaging;
using Gymunity.Domain.Entities.ProgramAggregate;

namespace Gymunity.Domain.Entities.Trainer
{
    public class TrainerProfile : BaseEntity
    {
        // Id comes from BaseEntity
        public string UserId { get; set; } = null!;
        public string Handle { get; set; } = null!; // @wahidfitness ==> unique
        public string Bio { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public string? VideoIntroUrl { get; set; }
        public string? BrandingColors { get; set; } // JSON or hex string
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        public bool IsSuspended { get; set; }
        public DateTime? SuspendedAt { get; set; }

        public decimal RatingAverage { get; set; } =0;
        public int TotalClients { get; set; } =0;
        public int YearsExperience { get; set; }

        // New Properties for Status
        public string? StatusImageUrl { get; set; }
        public string? StatusDescription { get; set; }

        // Navigation
        public AppUser User { get; set; } = null!;
        public ICollection<Program> Programs { get; set; } = [];
        public ICollection<Package> Packages { get; set; } = [];
        public ICollection<MessageThread> MessageThreadsAsTrainer { get; set; } = [];
        public ICollection<TrainerReview> TrainerReviews { get; set; } = [];
    }
}