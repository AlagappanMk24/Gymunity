namespace Gymunity.Admin.MVC.ViewModels.Dashboard.Components
{
    /// <summary>
    /// Represents a single statistics card on the dashboard.
    /// </summary>
    public class StatisticsCardViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public string? ColorClass { get; set; } // primary, success, danger, warning
        public decimal? Trend { get; set; }
        public bool IsTrendPositive { get; set; }
        public string? LinkUrl { get; set; }
    }
}
