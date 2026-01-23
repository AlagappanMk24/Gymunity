using Gymunity.Domain.Enums;

namespace Gymunity.Admin.MVC.ViewModels.Programs
{
    public class ProgramDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public ProgramType Type { get; set; }
        public int DurationWeeks { get; set; }
        public decimal? Price { get; set; }
        public bool IsPublic { get; set; }
        public int? MaxClients { get; set; }
        public string? ThumbnailUrl { get; set; }

        // Trainer Information
        public int? TrainerProfileId { get; set; }
        public string? TrainerUserName { get; set; }
        public string? TrainerHandle { get; set; }
        public string? TrainerEmail { get; set; }

        // Timestamps
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Statistics
        public int TotalWeeks { get; set; }
        public int TotalExercises { get; set; }
        public int TotalExerciseCount { get; set; }

        // Display Properties
        public string PublicStatus => IsPublic ? "Public" : "Private";
        public string ProgramTypeDisplay => Type.ToString();
    }
}