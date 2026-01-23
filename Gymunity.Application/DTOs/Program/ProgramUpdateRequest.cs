using Gymunity.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.Program
{
    public class ProgramUpdateRequest
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
        public string Title { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Range(1, 4, ErrorMessage = "Invalid program type.")]
        public ProgramType Type { get; set; }

        [Range(1, 520, ErrorMessage = "Duration (weeks) must be between 1 and 520.")]
        public int DurationWeeks { get; set; }

        [Range(0.01, 100000, ErrorMessage = "Price must be between 0.01 and 100000.")]
        public decimal? Price { get; set; }

        public bool IsPublic { get; set; }

        [Range(1, 10000, ErrorMessage = "Max clients must be between 1 and 10000.")]
        public int? MaxClients { get; set; }

        [Url(ErrorMessage = "Thumbnail URL is not valid.")]
        public string? ThumbnailUrl { get; set; }
    }
}