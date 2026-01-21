using Gymunity.Admin.MVC.ViewModels.Dashboard.Components;

namespace Gymunity.Admin.MVC.ViewModels.Dashboard
{
    /// <summary>
    /// Main dashboard view model containing all statistics and metrics.
    /// </summary>
    public class DashboardOverviewViewModel
    {
        // Primary Statistics
        public int TotalUsers { get; set; }
        public int TotalTrainers { get; set; }
        public int TotalClients { get; set; }
        public int ActiveSubscriptions { get; set; }
        public decimal TotalRevenue { get; set; }

        // Pending/Alert Metrics
        public int PendingReviews { get; set; }
        public int UnverifiedTrainers { get; set; }
        public int FailedPayments { get; set; }
        public int SystemHealth { get; set; } // Percentage 0-100

        // Trends (percentage change from previous period)
        public decimal UsersTrend { get; set; }
        public decimal TrainersTrend { get; set; }
        public decimal SubscriptionsTrend { get; set; }
        public decimal RevenueTrend { get; set; }

        // Chart Data
        public ChartDataViewModel? ChartData { get; set; }

        // Lists
        public List<StatisticsCardViewModel>? StatisticsCards { get; set; }
        public List<TopTrainerViewModel>? TopTrainers { get; set; }
        public List<RecentActivityViewModel>? RecentActivities { get; set; }

        public DashboardOverviewViewModel()
        {
            StatisticsCards = [];
            TopTrainers = [];
            RecentActivities = [];
        }
    }
}
