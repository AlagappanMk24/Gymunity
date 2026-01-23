using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.Trainers
{
    public class TrainerReviewCreateRequest
    {
        // ClientId removed intentionally: client user id will be taken from the authenticated user (authorization token)

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between1 and5")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment must not exceed1000 characters")]
        public string? Comment { get; set; }
    }
}