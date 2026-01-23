using Gymunity.Admin.MVC.Services.Interfaces;
using Gymunity.Admin.MVC.ViewModels.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.Admin.MVC.Controllers
{
    /// <summary>
    /// Analytics Controller for generating comprehensive business intelligence reports
    /// Provides detailed metrics, trends, and performance analysis
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class AnalyticsController(
        ILogger<AnalyticsController> logger,
        IAdminAnalyticsService analyticsService) : BaseAdminController
    {
        private readonly ILogger<AnalyticsController> _logger = logger;
        private readonly IAdminAnalyticsService _analyticsService = analyticsService;

        /// <summary>
        /// Displays the main analytics dashboard with comprehensive metrics
        /// Default period is last 30 days
        /// </summary>
        [HttpGet("analytics")]
        public async Task<IActionResult> Index([FromQuery] int days = 30, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                SetPageTitle("Analytics");
                SetBreadcrumbs("Business Analytics");

                // Determine date range
                DateTime actualEndDate = endDate ?? DateTime.UtcNow.Date.AddDays(1);
                DateTime actualStartDate = startDate ?? DateTime.UtcNow.Date.AddDays(-days);

                // Validate date range
                if (actualStartDate >= actualEndDate)
                {
                    ShowErrorMessage("Invalid date range. Start date must be before end date.");
                    actualStartDate = DateTime.UtcNow.Date.AddDays(-30);
                    actualEndDate = DateTime.UtcNow.Date.AddDays(1);
                }

                if ((actualEndDate - actualStartDate).Days > 365)
                {
                    ShowWarningMessage("Date range exceeds 1 year. Results may take longer to load.");
                }

                var model = await _analyticsService.GetAnalyticsOverviewAsync(actualStartDate, actualEndDate);

                _logger.LogInformation("Analytics dashboard accessed by user: {User} for period {Start} to {End}",
                    User.Identity?.Name, actualStartDate, actualEndDate);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading analytics dashboard");
                ShowErrorMessage("An error occurred while loading the analytics dashboard");
                return View(new AnalyticsOverviewViewModel());
            }
        }

        /// <summary>
        /// API endpoint to get revenue analytics data for chart rendering
        /// </summary>
        [HttpGet("analytics/api/revenue")]
        public async Task<IActionResult> GetRevenueData([FromQuery] int days = 30, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (days < 7 || days > 365) days = 30;

                DateTime actualEndDate = endDate ?? DateTime.UtcNow.Date.AddDays(1);
                DateTime actualStartDate = startDate ?? DateTime.UtcNow.Date.AddDays(-days);

                var model = await _analyticsService.GetAnalyticsOverviewAsync(actualStartDate, actualEndDate);

                return Ok(new
                {
                    success = true,
                    labels = model.RevenueChartLabels,
                    data = model.RevenueChartData,
                    totalRevenue = model.TotalRevenue,
                    averagePerDay = model.AverageRevenuePerDay,
                    growth = model.RevenueGrowth
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue data");
                return BadRequest(new { success = false, message = "Error retrieving revenue data" });
            }
        }

        /// <summary>
        /// API endpoint to get subscription analytics data
        /// </summary>
        [HttpGet("analytics/api/subscriptions")]
        public async Task<IActionResult> GetSubscriptionData([FromQuery] int days = 30, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (days < 7 || days > 365) days = 30;

                DateTime actualEndDate = endDate ?? DateTime.UtcNow.Date.AddDays(1);
                DateTime actualStartDate = startDate ?? DateTime.UtcNow.Date.AddDays(-days);

                var model = await _analyticsService.GetAnalyticsOverviewAsync(actualStartDate, actualEndDate);

                return Ok(new
                {
                    success = true,
                    labels = model.SubscriptionChartLabels,
                    data = model.SubscriptionChartData,
                    totalSubscriptions = model.TotalSubscriptions,
                    activeSubscriptions = model.ActiveSubscriptions,
                    growth = model.SubscriptionGrowth,
                    churnRate = model.ChurnRate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription data");
                return BadRequest(new { success = false, message = "Error retrieving subscription data" });
            }
        }

        /// <summary>
        /// API endpoint to get user acquisition data
        /// </summary>
        [HttpGet("analytics/api/users")]
        public async Task<IActionResult> GetUserData([FromQuery] int days = 30, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (days < 7 || days > 365) days = 30;

                DateTime actualEndDate = endDate ?? DateTime.UtcNow.Date.AddDays(1);
                DateTime actualStartDate = startDate ?? DateTime.UtcNow.Date.AddDays(-days);

                var model = await _analyticsService.GetAnalyticsOverviewAsync(actualStartDate, actualEndDate);

                return Ok(new
                {
                    success = true,
                    labels = model.UserChartLabels,
                    data = model.UserChartData,
                    totalNewUsers = model.TotalNewUsers,
                    newClients = model.TotalNewClients,
                    newTrainers = model.TotalNewTrainers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user data");
                return BadRequest(new { success = false, message = "Error retrieving user data" });
            }
        }

        /// <summary>
        /// API endpoint to get payment status breakdown
        /// </summary>
        [HttpGet("analytics/api/payment-status")]
        public async Task<IActionResult> GetPaymentStatusData([FromQuery] int days = 30, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (days < 7 || days > 365) days = 30;

                DateTime actualEndDate = endDate ?? DateTime.UtcNow.Date.AddDays(1);
                DateTime actualStartDate = startDate ?? DateTime.UtcNow.Date.AddDays(-days);

                var model = await _analyticsService.GetAnalyticsOverviewAsync(actualStartDate, actualEndDate);

                return Ok(new
                {
                    success = true,
                    labels = model.PaymentStatusLabels,
                    data = model.PaymentStatusData,
                    totalPayments = model.TotalPayments,
                    successRate = model.PaymentSuccessRate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status data");
                return BadRequest(new { success = false, message = "Error retrieving payment status data" });
            }
        }

        /// <summary>
        /// API endpoint to get subscription status breakdown
        /// </summary>
        [HttpGet("analytics/api/subscription-status")]
        public async Task<IActionResult> GetSubscriptionStatusData([FromQuery] int days = 30, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (days < 7 || days > 365) days = 30;

                DateTime actualEndDate = endDate ?? DateTime.UtcNow.Date.AddDays(1);
                DateTime actualStartDate = startDate ?? DateTime.UtcNow.Date.AddDays(-days);

                var model = await _analyticsService.GetAnalyticsOverviewAsync(actualStartDate, actualEndDate);

                return Ok(new
                {
                    success = true,
                    labels = model.SubscriptionStatusLabels,
                    data = model.SubscriptionStatusData,
                    activeSubscriptions = model.ActiveSubscriptions,
                    churnRate = model.ChurnRate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription status data");
                return BadRequest(new { success = false, message = "Error retrieving subscription status data" });
            }
        }

        /// <summary>
        /// API endpoint to get top trainers data
        /// </summary>
        [HttpGet("analytics/api/top-trainers")]
        public async Task<IActionResult> GetTopTrainersData([FromQuery] int days = 30, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (days < 7 || days > 365) days = 30;

                DateTime actualEndDate = endDate ?? DateTime.UtcNow.Date.AddDays(1);
                DateTime actualStartDate = startDate ?? DateTime.UtcNow.Date.AddDays(-days);

                var model = await _analyticsService.GetAnalyticsOverviewAsync(actualStartDate, actualEndDate);

                return Ok(new
                {
                    success = true,
                    trainers = model.TopTrainers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top trainers data");
                return BadRequest(new { success = false, message = "Error retrieving top trainers data" });
            }
        }

        /// <summary>
        /// API endpoint to get payment methods breakdown
        /// </summary>
        [HttpGet("analytics/api/payment-methods")]
        public async Task<IActionResult> GetPaymentMethodsData([FromQuery] int days = 30, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (days < 7 || days > 365) days = 30;

                DateTime actualEndDate = endDate ?? DateTime.UtcNow.Date.AddDays(1);
                DateTime actualStartDate = startDate ?? DateTime.UtcNow.Date.AddDays(-days);

                var model = await _analyticsService.GetAnalyticsOverviewAsync(actualStartDate, actualEndDate);

                return Ok(new
                {
                    success = true,
                    methods = model.PaymentMethods
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment methods data");
                return BadRequest(new { success = false, message = "Error retrieving payment methods data" });
            }
        }

        /// <summary>
        /// API endpoint to export analytics data (CSV format)
        /// </summary>
        [HttpGet("analytics/export")]
        public async Task<IActionResult> ExportAnalyticsData([FromQuery] int days = 30, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (days < 7 || days > 365) days = 30;

                DateTime actualEndDate = endDate ?? DateTime.UtcNow.Date.AddDays(1);
                DateTime actualStartDate = startDate ?? DateTime.UtcNow.Date.AddDays(-days);

                var model = await _analyticsService.GetAnalyticsOverviewAsync(actualStartDate, actualEndDate);

                // Generate CSV content
                var csv = GenerateAnalyticsCSV(model);
                var fileName = $"Analytics_{actualStartDate:yyyyMMdd}_{actualEndDate:yyyyMMdd}.csv";

                _logger.LogInformation("Analytics data exported by user: {User}", User.Identity?.Name);

                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting analytics data");
                ShowErrorMessage("An error occurred while exporting analytics data");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Generate CSV content from analytics model
        /// </summary>
        private string GenerateAnalyticsCSV(AnalyticsOverviewViewModel model)
        {
            var csv = new System.Text.StringBuilder();

            csv.AppendLine("ANALYTICS REPORT");
            csv.AppendLine($"Period: {model.PeriodLabel}");
            csv.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            csv.AppendLine();

            // Revenue Section
            csv.AppendLine("REVENUE METRICS");
            csv.AppendLine($"Total Revenue,EGP {model.TotalRevenue:N2}");
            csv.AppendLine($"Average Daily Revenue,EGP {model.AverageRevenuePerDay:N2}");
            csv.AppendLine($"Revenue Growth,%,{model.RevenueGrowth:N2}%");
            csv.AppendLine();

            // Payment Section
            csv.AppendLine("PAYMENT METRICS");
            csv.AppendLine($"Total Payments,{model.TotalPayments}");
            csv.AppendLine($"Completed,{model.CompletedPayments}");
            csv.AppendLine($"Failed,{model.FailedPayments}");
            csv.AppendLine($"Pending,{model.PendingPayments}");
            csv.AppendLine($"Success Rate,%,{model.PaymentSuccessRate:N2}%");
            csv.AppendLine();

            // Subscription Section
            csv.AppendLine("SUBSCRIPTION METRICS");
            csv.AppendLine($"Total Subscriptions,{model.TotalSubscriptions}");
            csv.AppendLine($"Active,{model.ActiveSubscriptions}");
            csv.AppendLine($"Canceled,{model.CanceledSubscriptions}");
            csv.AppendLine($"Unpaid,{model.UnpaidSubscriptions}");
            csv.AppendLine($"Churn Rate,%,{model.ChurnRate:N2}%");
            csv.AppendLine();

            // User Section
            csv.AppendLine("USER METRICS");
            csv.AppendLine($"New Users,{model.TotalNewUsers}");
            csv.AppendLine($"New Clients,{model.TotalNewClients}");
            csv.AppendLine($"New Trainers,{model.TotalNewTrainers}");
            csv.AppendLine();

            // Trainer Section
            csv.AppendLine("TRAINER METRICS");
            csv.AppendLine($"Verified Trainers,{model.VerifiedTrainers}");
            csv.AppendLine($"Pending Trainers,{model.PendingTrainers}");
            csv.AppendLine($"Average Rating,{model.AverageTrainerRating:N2}");
            csv.AppendLine();

            return csv.ToString();
        }
    }
}
