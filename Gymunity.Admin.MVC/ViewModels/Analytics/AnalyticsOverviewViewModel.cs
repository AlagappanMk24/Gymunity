namespace Gymunity.Admin.MVC.ViewModels.Analytics
{
    /// <summary>
    /// Main analytics overview view model containing all analytics metrics and data
    /// Implements comprehensive business intelligence data for reporting
    /// </summary>
    public class AnalyticsOverviewViewModel
    {
        // Time Period Selection
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PeriodLabel { get; set; } = "Last 30 Days";

        // Revenue Analytics
        public decimal TotalRevenue { get; set; }
        public decimal AverageRevenuePerDay { get; set; }
        public decimal MaxDailyRevenue { get; set; }
        public decimal MinDailyRevenue { get; set; }
        public decimal RevenueGrowth { get; set; } // Percentage

        // Payment Analytics
        public int TotalPayments { get; set; }
        public int CompletedPayments { get; set; }
        public int FailedPayments { get; set; }
        public int PendingPayments { get; set; }
        public decimal PaymentSuccessRate { get; set; } // Percentage

        // Subscription Analytics
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int CanceledSubscriptions { get; set; }
        public int UnpaidSubscriptions { get; set; }
        public decimal ChurnRate { get; set; } // Percentage
        public decimal SubscriptionGrowth { get; set; } // Percentage

        // User Analytics
        public int TotalNewUsers { get; set; }
        public int TotalNewClients { get; set; }
        public int TotalNewTrainers { get; set; }
        public decimal UserRetention { get; set; } // Percentage

        // Trainer Analytics
        public int VerifiedTrainers { get; set; }
        public int PendingTrainers { get; set; }
        public decimal AverageTrainerRating { get; set; }
        public decimal AverageTrainerClients { get; set; }
        public decimal TopTrainerEarnings { get; set; }

        // Package Analytics
        public int MostPopularPackageId { get; set; }
        public string MostPopularPackageName { get; set; }
        public int MostPopularPackageSales { get; set; }
        public List<PackageAnalyticsViewModel> PackageAnalytics { get; set; }

        // Payment Method Analytics
        public List<PaymentMethodAnalyticsViewModel> PaymentMethods { get; set; }

        // Chart Data
        public List<object> RevenueChartLabels { get; set; }
        public List<decimal> RevenueChartData { get; set; }
        public List<object> SubscriptionChartLabels { get; set; }
        public List<int> SubscriptionChartData { get; set; }
        public List<object> UserChartLabels { get; set; }
        public List<int> UserChartData { get; set; }
        public List<object> PaymentStatusLabels { get; set; }
        public List<int> PaymentStatusData { get; set; }
        public List<object> SubscriptionStatusLabels { get; set; }
        public List<int> SubscriptionStatusData { get; set; }

        // Top Performers
        public List<TopTrainerAnalyticsViewModel> TopTrainers { get; set; }
        public List<TopPackageAnalyticsViewModel> TopPackages { get; set; }

        public AnalyticsOverviewViewModel()
        {
            StartDate = DateTime.UtcNow.AddDays(-30);
            EndDate = DateTime.UtcNow;
            PackageAnalytics = new List<PackageAnalyticsViewModel>();
            PaymentMethods = new List<PaymentMethodAnalyticsViewModel>();
            RevenueChartLabels = new List<object>();
            RevenueChartData = new List<decimal>();
            SubscriptionChartLabels = new List<object>();
            SubscriptionChartData = new List<int>();
            UserChartLabels = new List<object>();
            UserChartData = new List<int>();
            PaymentStatusLabels = new List<object>();
            PaymentStatusData = new List<int>();
            SubscriptionStatusLabels = new List<object>();
            SubscriptionStatusData = new List<int>();
            TopTrainers = new List<TopTrainerAnalyticsViewModel>();
            TopPackages = new List<TopPackageAnalyticsViewModel>();
        }
    }

    /// <summary>
    /// Package analytics view model for tracking package performance
    /// </summary>
    public class PackageAnalyticsViewModel
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; }
        public decimal Price { get; set; }
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageSalesPerDay { get; set; }
        public int ActiveSubscriptions { get; set; }
        public decimal ConversionRate { get; set; }
        public int DurationInDays { get; set; }
    }

    /// <summary>
    /// Payment method analytics for understanding payment preferences
    /// </summary>
    public class PaymentMethodAnalyticsViewModel
    {
        public string MethodName { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AverageTransactionValue { get; set; }
        public int FailureCount { get; set; }
    }

    /// <summary>
    /// Top trainer analytics for performance tracking
    /// </summary>
    public class TopTrainerAnalyticsViewModel
    {
        public int TrainerId { get; set; }
        public string TrainerName { get; set; }
        public string Handle { get; set; }
        public int ClientCount { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal AverageRating { get; set; }
        public int SubscriptionCount { get; set; }
        public int ReviewCount { get; set; }
    }

    /// <summary>
    /// Top package analytics for product performance
    /// </summary>
    public class TopPackageAnalyticsViewModel
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; }
        public string TrainerName { get; set; }
        public int SalesCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public decimal Revenue { get; set; }
    }
}
