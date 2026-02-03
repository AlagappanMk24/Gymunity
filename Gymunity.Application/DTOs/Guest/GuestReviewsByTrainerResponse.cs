namespace Gymunity.Application.DTOs.Guest
{
    public class GuestReviewsByTrainerResponse
    {
        public int TrainerProfileId { get; set; }
        public GuestReviewResponseItem[] Reviews { get; set; } = new GuestReviewResponseItem[0];
    }
}