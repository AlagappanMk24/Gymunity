using Gymunity.Admin.MVC.ViewModels.Dashboard;
using Gymunity.Admin.MVC.ViewModels.Dashboard.Components;

namespace Gymunity.Admin.MVC.Services.Interfaces
{
    public interface IDashboardStatisticsService
    {
        Task<DashboardOverviewViewModel> GetDashboardOverviewAsync();
        Task<ChartDataViewModel> GetRevenueChartDataAsync(int days = 30);
        Task<ChartDataViewModel> GetSubscriptionGrowthChartDataAsync(int days = 30);
        Task<ChartDataViewModel> GetUserDistributionChartDataAsync();
        Task<ChartDataViewModel> GetPaymentStatusChartDataAsync();
        Task<ChartDataViewModel> GetSubscriptionStatusChartDataAsync();
    }
}