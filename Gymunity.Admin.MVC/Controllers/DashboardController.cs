using Gymunity.Admin.MVC.Services;
using Gymunity.Admin.MVC.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.Admin.MVC.Controllers
{
    /// <summary>
    /// Admin Dashboard Controller
    /// Displays key metrics, charts, and system overview.
    /// Provides dynamic data loading with aggregations and analytics.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class DashboardController(
        ILogger<DashboardController> logger,
        DashboardStatisticsService dashboardService) : BaseAdminController
    {
        private readonly ILogger<DashboardController> _logger = logger;
        private readonly DashboardStatisticsService _dashboardService = dashboardService;

        /// <summary>
        /// Displays the main dashboard with statistics and charts.
        /// </summary>
        [HttpGet("")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> Index()
        {
            try
            {
                SetPageTitle("Dashboard");
                SetBreadcrumbs();

                var model = await _dashboardService.GetDashboardOverviewAsync();

                _logger.LogInformation("Dashboard accessed by user: {User}", User.Identity?.Name);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                ShowErrorMessage("An error occurred while loading the dashboard");
                return View(new DashboardOverviewViewModel());
            }
        }

        /// <summary>
        /// Refreshes dashboard data (API endpoint).
        /// </summary>
        [HttpPost("dashboard/refresh")]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                var model = await _dashboardService.GetDashboardOverviewAsync();
                return Ok(new { success = true, message = "Dashboard refreshed successfully", data = model });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing dashboard");
                return BadRequest(new { success = false, message = "Error refreshing dashboard" });
            }
        }

        /// <summary>
        /// Gets dashboard statistics (API endpoint).
        /// </summary>
        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var model = await _dashboardService.GetDashboardOverviewAsync();
                return Ok(new
                {
                    totalUsers = model.TotalUsers,
                    totalClients = model.TotalClients,
                    totalTrainers = model.TotalTrainers,
                    unverifiedTrainers = model.UnverifiedTrainers,
                    activeSubscriptions = model.ActiveSubscriptions,
                    totalRevenue = model.TotalRevenue,
                    failedPayments = model.FailedPayments,
                    pendingReviews = model.PendingReviews,
                    systemHealth = model.SystemHealth,
                    usersTrend = model.UsersTrend,
                    trainersTrend = model.TrainersTrend,
                    subscriptionsTrend = model.SubscriptionsTrend,
                    revenueTrend = model.RevenueTrend
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics");
                return BadRequest(new { success = false, message = "Error getting statistics" });
            }
        }

        /// <summary>
        /// Gets chart data for revenue trends.
        /// </summary>
        [HttpGet("dashboard/chart-data/revenue")]
        public async Task<IActionResult> GetRevenueChartData([FromQuery] int days = 30)
        {
            try
            {
                if (days < 7 || days > 365)
                    days = 30;

                var chartData = await _dashboardService.GetRevenueChartDataAsync(days);
                return Ok(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue chart data");
                return BadRequest(new { success = false, message = "Error getting revenue chart data" });
            }
        }

        /// <summary>
        /// Gets subscription growth chart data.
        /// </summary>
        [HttpGet("dashboard/chart-data/subscriptions")]
        public async Task<IActionResult> GetSubscriptionChartData([FromQuery] int days = 30)
        {
            try
            {
                if (days < 7 || days > 365)
                    days = 30;

                var chartData = await _dashboardService.GetSubscriptionGrowthChartDataAsync(days);
                return Ok(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription chart data");
                return BadRequest(new { success = false, message = "Error getting subscription chart data" });
            }
        }

        /// <summary>
        /// Gets user distribution chart data.
        /// </summary>
        [HttpGet("dashboard/chart-data/users")]
        public async Task<IActionResult> GetUserDistributionChartData()
        {
            try
            {
                var chartData = await _dashboardService.GetUserDistributionChartDataAsync();
                return Ok(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user distribution data");
                return BadRequest(new { success = false, message = "Error getting user distribution data" });
            }
        }

        /// <summary>
        /// Gets payment status breakdown chart data.
        /// </summary>
        [HttpGet("dashboard/chart-data/payments")]
        public async Task<IActionResult> GetPaymentStatusChartData()
        {
            try
            {
                var chartData = await _dashboardService.GetPaymentStatusChartDataAsync();
                return Ok(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status data");
                return BadRequest(new { success = false, message = "Error getting payment status data" });
            }
        }

        /// <summary>
        /// Gets subscription status breakdown chart data.
        /// </summary>
        [HttpGet("dashboard/chart-data/subscription-status")]
        public async Task<IActionResult> GetSubscriptionStatusChartData()
        {
            try
            {
                var chartData = await _dashboardService.GetSubscriptionStatusChartDataAsync();
                return Ok(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription status data");
                return BadRequest(new { success = false, message = "Error getting subscription status data" });
            }
        }
    }
}
