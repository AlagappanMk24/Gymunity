namespace Gymunity.Application.DTOs.ClientDto
{
    public class ClientProfileDashboardResponse
    {
        // Header
        public string UserName { get; set; } = string.Empty;
        public string? Goal { get; set; }
        public string? ExperienceLevel { get; set; }
        public bool IsOnboardingCompleted { get; set; }

        // Body Stats (last log)
        public BodyStateLogResponse? LastBodyState { get; set; }

        // Progress (history)
        public List<BodyStateLogResponse> BodyStateHistory { get; set; } = new();

        // Meta
        public DateTime ProfileCreatedAt { get; set; }
    }
}