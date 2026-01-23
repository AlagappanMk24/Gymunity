namespace Gymunity.Application.DTOs.Program.ProgramDayDtos
{
    public class ProgramDayExerciseResponse
    {
        public int ProgramDayId { get; set; }
        public int ExerciseId { get; set; }
        public int OrderIndex { get; set; }
        // Standard fields
        public string? Sets { get; set; } // "3", "3-4", "5x5"
        public string? Reps { get; set; } // "8-12", "AMRAP"
        public int? RestSeconds { get; set; }
        public string? Tempo { get; set; } // "3010"
        public decimal? RPE { get; set; }
        public decimal? Percent1RM { get; set; }
        public string? Notes { get; set; }
        public string? VideoUrl { get; set; }

        // Flexible supersets/circuits/AMRAP
        public string? ExerciseDataJson { get; set; } // complex structures stored as JSON
        public string ExcersiceName { get; set; } = null!;
        public string Category { get; set; } = null!; // Strength, Cardio, etc.
        public string MuscleGroup { get; set; } = null!;
        public string? Equipment { get; set; }
        public string? VideoDemoUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool IsCustom { get; set; } = false;
        public string? TrainerId { get; set; } // null = global library
    }
}