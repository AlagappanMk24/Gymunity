namespace Gymunity.Admin.MVC.ViewModels.Dashboard.Components
{
    /// <summary>
    /// Represents a recent activity item on the dashboard.
    /// </summary>
    public class RecentActivityViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IconClass { get; set; }
        public string? ColorClass { get; set; }
        public DateTime Timestamp { get; set; }
        public string TimeAgo => GetTimeAgo(Timestamp);

        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalSeconds < 60)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes > 1 ? "s" : "")} ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours > 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays > 1 ? "s" : "")} ago";

            return dateTime.ToString("MMM dd, yyyy");
        }
    }
}
