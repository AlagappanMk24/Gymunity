namespace Gymunity.Application.DTOs.ExerciseLibrary
{
    public class ExerciseGetAllResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string MuscleGroup { get; set; } = null!;
        public string? Equipment { get; set; }
        public string? VideoDemoUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool IsCustom { get; set; }
        public string? TrainerId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}