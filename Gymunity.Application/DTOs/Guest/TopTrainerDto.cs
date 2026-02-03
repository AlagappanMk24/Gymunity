namespace Gymunity.Application.DTOs.Guest
{
    public class TopTrainerResponse
    {
        public int TrainerProfileId { get; set; }
        public string Handle { get; set; } = string.Empty;
        public int TotalClients { get; set; }
    }
}