using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.ExerciseLibrary
{
    public class ExerciseCreateRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between2 and200 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(100, ErrorMessage = "Category must be at most100 characters.")]
        public string Category { get; set; } = null!;

        [Required(ErrorMessage = "Muscle group is required.")]
        [StringLength(100, ErrorMessage = "Muscle group must be at most100 characters.")]
        public string MuscleGroup { get; set; } = null!;

        [StringLength(200, ErrorMessage = "Equipment must be at most200 characters.")]
        public string? Equipment { get; set; }

        [Url(ErrorMessage = "Video demo URL is not valid.")]
        public string? VideoDemoUrl { get; set; }

        [Url(ErrorMessage = "Thumbnail URL is not valid.")]
        public string? ThumbnailUrl { get; set; }

        public bool IsCustom { get; set; } = true;

        [StringLength(450, ErrorMessage = "TrainerId length must be450 characters or fewer.")]
        public string? TrainerId { get; set; }
    }
}