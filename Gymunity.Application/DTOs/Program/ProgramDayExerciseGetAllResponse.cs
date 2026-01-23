namespace Gymunity.Application.DTOs.Program
{
    public class ProgramDayExerciseGetAllResponse
    {
        public int Id { get; set; }
        public int ProgramDayId { get; set; }
        public int ExerciseId { get; set; }
        public int OrderIndex { get; set; }
        public string? Sets { get; set; }
        public string? Reps { get; set; }
        public int? RestSeconds { get; set; }
        public string? Tempo { get; set; }
        public decimal? RPE { get; set; }
        public decimal? Percent1RM { get; set; }
        public string? Notes { get; set; }
        public string? VideoUrl { get; set; }
        public string? ExerciseDataJson { get; set; }
    }
}