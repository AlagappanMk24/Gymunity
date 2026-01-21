using Gymunity.Domain.Entities.Client;

namespace Gymunity.Domain.Entities.Trainer
{
    public class TrainerReview : BaseEntity
    {
        // main relationships
        public int TrainerId { get; set; }
        public TrainerProfile Trainer { get; set; } = null!;

        public int ClientId { get; set; }
        public ClientProfile Client { get; set; } = null!;

        public int Rating { get; set; }          // 1 → 5
        public string? Comment { get; set; }    
        public bool IsEdited { get; set; }
        public DateTimeOffset? EditedAt { get; set; }

        public bool IsApproved { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
    }
}