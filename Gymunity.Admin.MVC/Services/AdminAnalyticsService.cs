using Gymunity.Admin.MVC.Services.Interfaces;
using Gymunity.Admin.MVC.ViewModels.Analytics;
using Gymunity.Application.Specifications.Admin;
using Gymunity.Domain;
using Gymunity.Domain.Entities;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Enums;
using DomainPayment = Gymunity.Domain.Entities.Payment;
namespace Gymunity.Admin.MVC.Services
{

    /// <summary>
    /// Service for retrieving comprehensive analytics and business intelligence data
    /// Implements aggregation, trends, and performance metrics
    /// Follows repository pattern and unit of work for data access
    /// </summary>
    public class AdminAnalyticsService(
        IUnitOfWork unitOfWork,
        ILogger<AdminAnalyticsService> logger) : IAdminAnalyticsService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<AdminAnalyticsService> _logger = logger;

        /// <summary>
        /// Get comprehensive analytics overview for specified period
        /// Aggregates revenue, payment, subscription, and user metrics
        /// </summary>
        public async Task<AnalyticsOverviewViewModel> GetAnalyticsOverviewAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var model = new AnalyticsOverviewViewModel
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    PeriodLabel = GetPeriodLabel(startDate, endDate)
                };

                // Get all necessary data with specifications
                var paymentSpec = new DashboardPaymentAnalyticsSpecs(startDate, endDate);
                var subscriptionSpec = new DashboardSubscriptionAnalyticsSpecs(startDate, endDate);
                var trainerSpec = new DashboardTrainerAnalyticsSpecs();

                var payments = (await _unitOfWork.Repository<DomainPayment>().GetAllWithSpecsAsync(paymentSpec)).ToList();
                var subscriptions = (await _unitOfWork.Repository<Subscription>().GetAllWithSpecsAsync(subscriptionSpec)).ToList();
                var trainers = (await _unitOfWork.Repository<TrainerProfile>().GetAllWithSpecsAsync(trainerSpec)).ToList();

                // Calculate Revenue Metrics
                await CalculateRevenueMetricsAsync(model, payments, startDate, endDate);

                // Calculate Payment Metrics
                CalculatePaymentMetrics(model, payments);

                // Calculate Subscription Metrics
                await CalculateSubscriptionMetricsAsync(model, subscriptions, startDate, endDate);

                // Calculate User Metrics
                await CalculateUserMetricsAsync(model, subscriptions, startDate, endDate);

                // Calculate Trainer Metrics
                CalculateTrainerMetrics(model, trainers);

                // Calculate Package Analytics
                await CalculatePackageAnalyticsAsync(model, subscriptions);

                // Calculate Payment Method Analytics
                CalculatePaymentMethodAnalytics(model, payments);

                // Get Top Performers
                model.TopTrainers = GetTopTrainers(trainers);
                model.TopPackages = await GetTopPackagesAsync(subscriptions);

                _logger.LogInformation("Analytics overview generated successfully for period {Start} to {End}",
                    startDate, endDate);

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating analytics overview");
                throw;
            }
        }

        /// <summary>
        /// Calculate revenue metrics including total, average, growth
        /// </summary>
        private async Task CalculateRevenueMetricsAsync(
            AnalyticsOverviewViewModel model,
            List<DomainPayment> payments,
            DateTime startDate,
            DateTime endDate)
        {
            try
            {
                var completedPayments = payments.Where(p => p.Status == PaymentStatus.Completed).ToList();

                model.TotalRevenue = completedPayments.Sum(p => p.Amount);

                var dayCount = (endDate - startDate).Days + 1;
                model.AverageRevenuePerDay = dayCount > 0 ? model.TotalRevenue / dayCount : 0;

                if (completedPayments.Any())
                {
                    var dailyRevenue = completedPayments
                        .GroupBy(p => p.PaidAt?.Date ?? p.CreatedAt.Date)
                        .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

                    model.MaxDailyRevenue = dailyRevenue.Any() ? dailyRevenue.Values.Max() : 0;
                    model.MinDailyRevenue = dailyRevenue.Any() ? dailyRevenue.Values.Where(v => v > 0).DefaultIfEmpty(0).Min() : 0;
                }

                // Calculate growth rate
                var previousPeriodStart = startDate.AddDays(-dayCount);
                var previousPeriodEnd = startDate.AddSeconds(-1);

                var previousSpec = new DashboardPaymentAnalyticsSpecs(previousPeriodStart, previousPeriodEnd);
                var previousPayments = await _unitOfWork.Repository<DomainPayment>()
                    .GetAllWithSpecsAsync(previousSpec);

                var previousRevenue = previousPayments
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .Sum(p => p.Amount);

                if (previousRevenue > 0)
                {
                    model.RevenueGrowth = ((model.TotalRevenue - previousRevenue) / previousRevenue) * 100;
                }

                // Build revenue chart data
                var dailyRevenueDict = completedPayments
                    .GroupBy(p => p.PaidAt?.Date ?? p.CreatedAt.Date)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

                for (int i = 0; i < dayCount; i++)
                {
                    var date = startDate.AddDays(i);
                    model.RevenueChartLabels.Add(date.ToString("MMM dd"));
                    var value = dailyRevenueDict.ContainsKey(date) ? dailyRevenueDict[date] : 0m;
                    model.RevenueChartData.Add(value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating revenue metrics");
            }
        }

        /// <summary>
        /// Calculate payment status and performance metrics
        /// </summary>
        private void CalculatePaymentMetrics(AnalyticsOverviewViewModel model, List<DomainPayment> payments)
        {
            try
            {
                model.TotalPayments = payments.Count;
                model.CompletedPayments = payments.Count(p => p.Status == PaymentStatus.Completed);
                model.FailedPayments = payments.Count(p => p.Status == PaymentStatus.Failed);
                model.PendingPayments = payments.Count(p => p.Status == PaymentStatus.Pending);

                if (model.TotalPayments > 0)
                {
                    model.PaymentSuccessRate = (model.CompletedPayments / (decimal)model.TotalPayments) * 100;
                }

                // Payment status chart
                model.PaymentStatusLabels = new List<object> { "Completed", "Failed", "Pending" };
                model.PaymentStatusData = new List<int>
                {
                    model.CompletedPayments,
                    model.FailedPayments,
                    model.PendingPayments
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating payment metrics");
            }
        }

        /// <summary>
        /// Calculate subscription metrics and trends
        /// </summary>
        private async Task CalculateSubscriptionMetricsAsync(
            AnalyticsOverviewViewModel model,
            List<Subscription> subscriptions,
            DateTime startDate,
            DateTime endDate)
        {
            try
            {
                var activeSubscriptions = subscriptions.Where(s => s.Status == SubscriptionStatus.Active).ToList();
                var canceledSubscriptions = subscriptions.Where(s => s.Status == SubscriptionStatus.Canceled).ToList();

                model.TotalSubscriptions = subscriptions.Count;
                model.ActiveSubscriptions = activeSubscriptions.Count;
                model.CanceledSubscriptions = canceledSubscriptions.Count;
                model.UnpaidSubscriptions = subscriptions.Count(s => s.Status == SubscriptionStatus.Unpaid);

                // Calculate churn rate
                if (model.TotalSubscriptions > 0)
                {
                    model.ChurnRate = (model.CanceledSubscriptions / (decimal)model.TotalSubscriptions) * 100;
                }

                // Calculate subscription growth
                var dayCount = (endDate - startDate).Days + 1;
                var previousPeriodStart = startDate.AddDays(-dayCount);
                var previousPeriodEnd = startDate.AddSeconds(-1);

                var previousSpec = new DashboardSubscriptionAnalyticsSpecs(previousPeriodStart, previousPeriodEnd);
                var previousSubscriptions = (await _unitOfWork.Repository<Subscription>()
                    .GetAllWithSpecsAsync(previousSpec)).ToList();

                if (previousSubscriptions.Any())
                {
                    model.SubscriptionGrowth = ((model.TotalSubscriptions - previousSubscriptions.Count()) /
                        (decimal)previousSubscriptions.Count()) * 100;
                }

                // Build subscription chart data
                var dailySubscriptions = subscriptions
                    .GroupBy(s => s.CreatedAt.Date)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.Count());

                for (int i = 0; i < dayCount; i++)
                {
                    var date = startDate.AddDays(i);
                    model.SubscriptionChartLabels.Add(date.ToString("MMM dd"));
                    var value = dailySubscriptions.ContainsKey(date) ? dailySubscriptions[date] : 0;
                    model.SubscriptionChartData.Add(value);
                }

                // Subscription status chart
                model.SubscriptionStatusLabels = new List<object> { "Active", "Unpaid", "Canceled" };
                model.SubscriptionStatusData = new List<int>
                {
                    model.ActiveSubscriptions,
                    model.UnpaidSubscriptions,
                    model.CanceledSubscriptions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating subscription metrics");
            }
        }

        /// <summary>
        /// Calculate user acquisition and retention metrics
        /// </summary>
        private async Task CalculateUserMetricsAsync(
            AnalyticsOverviewViewModel model,
            List<Subscription> subscriptions,
            DateTime startDate,
            DateTime endDate)
        {
            try
            {
                // Count new clients in period
                var newClientIds = subscriptions
                    .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate)
                    .Select(s => s.ClientId)
                    .Distinct()
                    .Count();

                model.TotalNewClients = newClientIds;

                // Get all trainers
                var trainers = await _unitOfWork.Repository<TrainerProfile>().GetAllAsync();
                var newTrainers = trainers
                    .Where(t => t.VerifiedAt.HasValue &&
                               t.VerifiedAt >= startDate &&
                               t.VerifiedAt <= endDate)
                    .Count();

                model.TotalNewTrainers = newTrainers;
                model.TotalNewUsers = model.TotalNewClients + model.TotalNewTrainers;

                // Build user chart data
                var dayCount = (endDate - startDate).Days + 1;
                var dailyUsers = subscriptions
                    .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate)
                    .GroupBy(s => s.CreatedAt.Date)
                    .Select(g => new { g.Key, Count = g.Select(s => s.ClientId).Distinct().Count() })
                    .OrderBy(x => x.Key)
                    .ToDictionary(x => x.Key, x => x.Count);

                for (int i = 0; i < dayCount; i++)
                {
                    var date = startDate.AddDays(i);
                    model.UserChartLabels.Add(date.ToString("MMM dd"));
                    var value = dailyUsers.ContainsKey(date) ? dailyUsers[date] : 0;
                    model.UserChartData.Add(value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating user metrics");
            }
        }

        /// <summary>
        /// Calculate trainer-related metrics and performance
        /// </summary>
        private void CalculateTrainerMetrics(AnalyticsOverviewViewModel model, List<TrainerProfile> trainers)
        {
            try
            {
                model.VerifiedTrainers = trainers.Count(t => t.IsVerified);
                model.PendingTrainers = trainers.Count(t => !t.IsVerified);

                if (trainers.Any())
                {
                    model.AverageTrainerRating = trainers.Average(t => t.RatingAverage);

                    // Note: TotalClients is computed dynamically by counting distinct active subscribers
                    // For accurate metrics, use the DistinctClientCount from subscriptions
                    // This is a summary metric and will be computed at the service level
                    model.AverageTrainerClients = (decimal)trainers.Average(t => t.TotalClients);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating trainer metrics");
            }
        }

        /// <summary>
        /// Calculate package performance and analytics
        /// </summary>
        private async Task CalculatePackageAnalyticsAsync(
            AnalyticsOverviewViewModel model,
            List<Subscription> subscriptions)
        {
            try
            {
                var packages = await _unitOfWork.Repository<Package>().GetAllAsync();

                foreach (var package in packages)
                {
                    var packageSubscriptions = subscriptions.Where(s => s.PackageId == package.Id).ToList();
                    var packageRevenue = packageSubscriptions.Sum(s => package.PriceMonthly);
                    var activeCount = packageSubscriptions.Count(s => s.Status == SubscriptionStatus.Active);

                    var analytics = new PackageAnalyticsViewModel
                    {
                        PackageId = package.Id,
                        PackageName = package.Name,
                        Price = package.PriceMonthly,
                        TotalSales = packageSubscriptions.Count,
                        TotalRevenue = packageRevenue,
                        AverageSalesPerDay = packageSubscriptions.Count > 0 ?
                            packageSubscriptions.Count / (decimal)(model.EndDate - model.StartDate).Days : 0,
                        ActiveSubscriptions = activeCount,
                        DurationInDays = 30 // Default duration, can be enhanced with actual data
                    };

                    model.PackageAnalytics.Add(analytics);
                }

                // Set most popular package
                var mostPopular = model.PackageAnalytics.OrderByDescending(p => p.TotalSales).FirstOrDefault();
                if (mostPopular != null)
                {
                    model.MostPopularPackageId = mostPopular.PackageId;
                    model.MostPopularPackageName = mostPopular.PackageName;
                    model.MostPopularPackageSales = mostPopular.TotalSales;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating package analytics");
            }
        }

        /// <summary>
        /// Calculate payment method preferences and performance
        /// </summary>
        private void CalculatePaymentMethodAnalytics(AnalyticsOverviewViewModel model, List<DomainPayment> payments)
        {
            try
            {
                var groupedByMethod = payments.GroupBy(p => p.Method).ToList();

                foreach (var methodGroup in groupedByMethod)
                {
                    var methodPayments = methodGroup.ToList();
                    var completedPayments = methodPayments.Where(p => p.Status == PaymentStatus.Completed).ToList();

                    var analytics = new PaymentMethodAnalyticsViewModel
                    {
                        MethodName = methodGroup.Key.ToString(),
                        TransactionCount = methodPayments.Count,
                        TotalAmount = completedPayments.Sum(p => p.Amount),
                        FailureCount = methodPayments.Count(p => p.Status == PaymentStatus.Failed),
                        AverageTransactionValue = completedPayments.Count > 0 ?
                            completedPayments.Sum(p => p.Amount) / completedPayments.Count : 0
                    };

                    if (methodPayments.Count > 0)
                    {
                        analytics.SuccessRate = (completedPayments.Count / (decimal)methodPayments.Count) * 100;
                    }

                    model.PaymentMethods.Add(analytics);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating payment method analytics");
            }
        }

        /// <summary>
        /// Get top performing trainers
        /// </summary>
        private List<TopTrainerAnalyticsViewModel> GetTopTrainers(List<TrainerProfile> trainers)
        {
            try
            {
                return trainers
                    .OrderByDescending(t => t.RatingAverage)
                    .ThenByDescending(t => t.TotalClients)
                    .Take(5)
                    .Select(t => new TopTrainerAnalyticsViewModel
                    {
                        TrainerId = t.Id,
                        TrainerName = t.User?.FullName ?? t.Handle,
                        Handle = t.Handle,
                        ClientCount = t.TotalClients,
                        TotalEarnings = 0,
                        AverageRating = t.RatingAverage
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top trainers");
                return new List<TopTrainerAnalyticsViewModel>();
            }
        }

        /// <summary>
        /// Get top performing packages
        /// </summary>
        private async Task<List<TopPackageAnalyticsViewModel>> GetTopPackagesAsync(List<Subscription> subscriptions)
        {
            try
            {
                var packages = await _unitOfWork.Repository<Package>().GetAllAsync();

                return subscriptions
                    .GroupBy(s => s.PackageId)
                    .Select(g => new
                    {
                        PackageId = g.Key,
                        Count = g.Count(),
                        Revenue = g.Sum(s => packages.FirstOrDefault(p => p.Id == g.Key)?.PriceMonthly ?? 0)
                    })
                    .OrderByDescending(x => x.Revenue)
                    .Take(5)
                    .Select(x => new TopPackageAnalyticsViewModel
                    {
                        PackageId = x.PackageId,
                        PackageName = packages.FirstOrDefault(p => p.Id == x.PackageId)?.Name ?? "Unknown",
                        SalesCount = x.Count,
                        TotalRevenue = x.Revenue,
                        Revenue = x.Revenue
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top packages");
                return new List<TopPackageAnalyticsViewModel>();
            }
        }

        /// <summary>
        /// Generate user-friendly period label
        /// </summary>
        private string GetPeriodLabel(DateTime startDate, DateTime endDate)
        {
            var days = (endDate - startDate).Days;

            if (days == 7) return "Last 7 Days";
            if (days == 30) return "Last 30 Days";
            if (days == 90) return "Last 90 Days";
            if (days == 365) return "Last Year";

            return $"{startDate:MMM dd} - {endDate:MMM dd}";
        }
    }
}
