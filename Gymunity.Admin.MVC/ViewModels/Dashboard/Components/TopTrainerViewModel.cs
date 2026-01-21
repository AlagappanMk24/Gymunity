namespace Gymunity.Admin.MVC.ViewModels.Dashboard.Components
{
    /// <summary>
    /// Represents a trainer in the top trainers list.
    /// </summary>
    public class TopTrainerViewModel
    {
        public int TrainerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Handle { get; set; }
        public int ClientCount { get; set; }
        public decimal Rating { get; set; }
        public int YearsExperience { get; set; }
        public string? ImageUrl { get; set; }
    }
}
