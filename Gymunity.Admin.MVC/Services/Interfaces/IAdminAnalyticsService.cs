using Gymunity.Admin.MVC.ViewModels.Analytics;

namespace Gymunity.Admin.MVC.Services.Interfaces
{
    public interface IAdminAnalyticsService
    {
        Task<AnalyticsOverviewViewModel> GetAnalyticsOverviewAsync(DateTime startDate, DateTime endDate);
    }
}
