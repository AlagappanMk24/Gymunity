namespace Gymunity.Admin.MVC.ViewModels.Dashboard.Components
{
    /// <summary>
    /// Data for rendering charts on the dashboard.
    /// </summary>
    public class ChartDataViewModel
    {
        public List<string> Labels { get; set; } = new();
        public List<object> Data { get; set; } = new();
        public List<ChartDatasetViewModel> Datasets { get; set; } = new();
    }

    public class ChartDatasetViewModel
    {
        public string Label { get; set; } = string.Empty;
        public List<decimal> Data { get; set; } = new();
        public List<object>? ObjectData { get; set; }
        public string? BackgroundColor { get; set; }
        public string? BorderColor { get; set; }
        public int BorderWidth { get; set; } = 1;
        public double? Tension { get; set; }
        public bool Fill { get; set; } = true;
    }
}
