using Gymunity.Domain.Entities.Client;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymunity.Domain.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = null!;
        public string? ProfilePhotoUrl { get; set; }
        public UserRole Role { get; set; } = UserRole.Client;
        public bool IsVerified { get; set; } = false;
        public string? StripeCustomerId { get; set; }
        public string? StripeConnectAccountId { get; set; } // Only for trainers
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        [NotMapped]
        public int LoginCount { get; set; } = 0;

        // Navigation
        public TrainerProfile? TrainerProfile { get; set; }
        public ClientProfile? ClientProfile { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        //public ICollection<WorkoutLog> WorkoutLogs { get; set; } = new List<WorkoutLog>();
        //public ICollection<BodyStatLog> BodyStatLogs { get; set; } = new List<BodyStatLog>();
    }
}
