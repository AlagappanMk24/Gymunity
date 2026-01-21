namespace Gymunity.Application.DTOs.Email
{
    public class TrainerNotificationEmail
    {
        public string TrainerName { get; set; } = null!;
        public string TrainerEmail { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public string PackageName { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal TrainerPayout { get; set; }
        public string Currency { get; set; } = null!;
    }
}