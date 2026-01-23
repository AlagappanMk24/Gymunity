namespace Gymunity.Application.DTOs.Trainers
{
    public class TrainerCardDto
    {
        /// <summary>
        /// Trainer user ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Full name of the trainer
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Unique handle/username (e.g., @john_fitness)
        /// </summary>
        public string Handle { get; set; }

        /// <summary>
        /// Profile photo URL
        /// </summary>
        public string ProfilePhotoUrl { get; set; }

        /// <summary>
        /// Cover/banner image URL
        /// </summary>
        public string CoverImageUrl { get; set; }

        /// <summary>
        /// Short bio/description
        /// </summary>
        public string Bio { get; set; }

        /// <summary>
        /// Is this trainer verified by the platform?
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// Average rating (0-5 stars)
        /// </summary>
        public decimal RatingAverage { get; set; }

        /// <summary>
        /// Total number of reviews
        /// </summary>
        public int TotalReviews { get; set; }

        /// <summary>
        /// Total number of active clients
        /// </summary>
        public int TotalClients { get; set; }

        /// <summary>
        /// Years of experience as a trainer
        /// </summary>
        public int YearsExperience { get; set; }

        /// <summary>
        /// List of specializations (e.g., "Strength Training", "Weight Loss", "Bodybuilding")
        /// </summary>
        public List<string> Specializations { get; set; }

        /// <summary>
        /// Starting price for trainer's packages (lowest package price)
        /// Null if trainer has no packages
        /// </summary>
        public decimal? StartingPrice { get; set; }

        /// <summary>
        /// Currency code (e.g., "EGP", "USD")
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Does the current client have an active subscription with this trainer?
        /// </summary>
        public bool HasActiveSubscription { get; set; }
    }
}