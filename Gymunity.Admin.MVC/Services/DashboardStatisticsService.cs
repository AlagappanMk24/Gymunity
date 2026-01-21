using Gymunity.Admin.MVC.Services.Interfaces;
using Gymunity.Admin.MVC.ViewModels.Dashboard;
using Gymunity.Admin.MVC.ViewModels.Dashboard.Components;
using Gymunity.Application.Specifications.Admin;
using Gymunity.Domain;
using Gymunity.Domain.Entities;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using DomainPayment = Gymunity.Domain.Entities.Payment;
namespace Gymunity.Admin.MVC.Services
{
    /// <summary>
    /// Service for retrieving dashboard statistics and analysis
    /// Implements aggregation and analytics with efficient database queries
    /// </summary>
    public class DashboardStatisticsService(
        IUnitOfWork unitOfWork,
        ILogger<DashboardStatisticsService> logger) : IDashboardStatisticsService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<DashboardStatisticsService> _logger = logger;

        /// <summary>
        /// Get complete dashboard overview with all statistics
        /// Uses specifications for efficient data loading
        /// </summary>
        public async Task<DashboardOverviewViewModel> GetDashboardOverviewAsync()
        {
            try
            {
                var model = new DashboardOverviewViewModel();

                // Get subscription analytics
                var subscriptionsSpec = new DashboardSubscriptionAnalyticsSpecs();
                var allSubscriptions = await _unitOfWork
                    .Repository<Subscription>()
                    .GetAllWithSpecsAsync(subscriptionsSpec);

                var activeSubscriptions = allSubscriptions
                    .Where(s => s.Status == SubscriptionStatus.Active)
                    .ToList();

                model.ActiveSubscriptions = activeSubscriptions.Count;
                model.SubscriptionsTrend = await CalculateSubscriptionTrendAsync();

                // Get payment analytics
                var paymentSpec = new DashboardPaymentAnalyticsSpecs();
                var allPayments = await _unitOfWork
                    .Repository<DomainPayment>()
                    .GetAllWithSpecsAsync(paymentSpec);

                var completedPayments = allPayments
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .ToList();

                model.TotalRevenue = completedPayments.Sum(p => p.Amount);
                model.RevenueTrend = await CalculateRevenueTrendAsync();

                // Get trainer analytics
                var trainersSpec = new DashboardTrainerAnalyticsSpecs();
                var allTrainers = await _unitOfWork
                    .Repository<TrainerProfile>()
                    .GetAllWithSpecsAsync(trainersSpec);

                var verifiedTrainers = allTrainers.Where(t => t.IsVerified).ToList();
                model.TotalTrainers = verifiedTrainers.Count;
                model.UnverifiedTrainers = allTrainers.Count() - model.TotalTrainers;
                model.TrainersTrend = await CalculateTrainerTrendAsync();

                // Calculate total users (clients + trainers)
                var uniqueClients = allSubscriptions
                    .Select(s => s.ClientId)
                    .Distinct()
                    .Count();

                model.TotalUsers = uniqueClients + model.TotalTrainers;
                model.TotalClients = uniqueClients;
                model.UsersTrend = await CalculateUserTrendAsync();

                // Get pending/alert metrics
                model.PendingReviews = await GetPendingReviewsCountAsync();
                model.FailedPayments = allPayments.Count(p => p.Status == PaymentStatus.Failed);
                model.SystemHealth = 98; // Default system health

                // Populate statistics cards
                model.StatisticsCards = GetStatisticsCards(model);

                // Get top trainers
                model.TopTrainers = GetTopTrainers(verifiedTrainers);

                // Get recent activities
                model.RecentActivities = GetRecentActivities(
                    allSubscriptions.ToList(), 
                    allPayments.ToList());

                _logger.LogInformation("Dashboard overview generated successfully");
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard overview");
                throw;
            }
        }

        /// <summary>
        /// Calculate subscription trend (percentage change from last period)
        /// </summary>
        private async Task<decimal> CalculateSubscriptionTrendAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var currentPeriodStart = now.AddDays(-30);
                var previousPeriodStart = now.AddDays(-60);

                var currentSpec = new DashboardSubscriptionAnalyticsSpecs(currentPeriodStart, now);
                var previousSpec = new DashboardSubscriptionAnalyticsSpecs(previousPeriodStart, currentPeriodStart);

                var currentCount = await _unitOfWork
                    .Repository<Subscription>()
                    .GetCountWithspecsAsync(currentSpec);

                var previousCount = await _unitOfWork
                    .Repository<Subscription>()
                    .GetCountWithspecsAsync(previousSpec);

                if (previousCount == 0) return 0;
                return (currentCount - previousCount) / (decimal)previousCount * 100;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating subscription trend");
                return 0;
            }
        }

        /// <summary>
        /// Calculate revenue trend (percentage change from last period)
        /// </summary>
        private async Task<decimal> CalculateRevenueTrendAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var currentPeriodStart = now.AddDays(-30);
                var previousPeriodStart = now.AddDays(-60);

                var currentSpec = new DashboardPaymentAnalyticsSpecs(currentPeriodStart, now);
                var previousSpec = new DashboardPaymentAnalyticsSpecs(previousPeriodStart, currentPeriodStart);

                var currentPayments = await _unitOfWork
                    .Repository<DomainPayment>()
                    .GetAllWithSpecsAsync(currentSpec);

                var previousPayments = await _unitOfWork
                    .Repository<DomainPayment>()
                    .GetAllWithSpecsAsync(previousSpec);

                var currentRevenue = currentPayments
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .Sum(p => p.Amount);

                var previousRevenue = previousPayments
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .Sum(p => p.Amount);

                if (previousRevenue == 0) return 0;
                return (currentRevenue - previousRevenue) / previousRevenue * 100;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating revenue trend");
                return 0;
            }
        }

        /// <summary>
        /// Calculate trainer trend (percentage change)
        /// </summary>
        private async Task<decimal> CalculateTrainerTrendAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var currentPeriodStart = now.AddDays(-30);
                var previousPeriodStart = now.AddDays(-60);

                var allTrainers = await _unitOfWork
                    .Repository<TrainerProfile>()
                    .GetAllAsync();

                var currentCount = allTrainers
                    .Where(t => t.VerifiedAt.HasValue && t.VerifiedAt >= currentPeriodStart)
                    .Count();

                var previousCount = allTrainers
                    .Where(t => t.VerifiedAt.HasValue && 
                               t.VerifiedAt >= previousPeriodStart && 
                               t.VerifiedAt < currentPeriodStart)
                    .Count();

                if (previousCount == 0) return 0;
                return (currentCount - previousCount) / (decimal)previousCount * 100;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating trainer trend");
                return 0;
            }
        }

        /// <summary>
        /// Calculate user trend (percentage change)
        /// </summary>
        private async Task<decimal> CalculateUserTrendAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var currentPeriodStart = now.AddDays(-30);
                var previousPeriodStart = now.AddDays(-60);

                var currentSpec = new DashboardSubscriptionAnalyticsSpecs(currentPeriodStart, now);
                var previousSpec = new DashboardSubscriptionAnalyticsSpecs(previousPeriodStart, currentPeriodStart);

                var currentSubscriptions = await _unitOfWork
                    .Repository<Subscription>()
                    .GetAllWithSpecsAsync(currentSpec);

                var previousSubscriptions = await _unitOfWork
                    .Repository<Subscription>()
                    .GetAllWithSpecsAsync(previousSpec);

                var currentUsers = currentSubscriptions
                    .Select(s => s.ClientId)
                    .Distinct()
                    .Count();

                var previousUsers = previousSubscriptions
                    .Select(s => s.ClientId)
                    .Distinct()
                    .Count();

                if (previousUsers == 0) return 0;
                return (currentUsers - previousUsers) / (decimal)previousUsers * 100;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating user trend");
                return 0;
            }
        }

        /// <summary>
        /// Get count of pending reviews
        /// </summary>
        private async Task<int> GetPendingReviewsCountAsync()
        {
            try
            {
                var reviews = await _unitOfWork.Repository<TrainerReview>().GetAllAsync();
                return reviews.Count(r => !r.IsApproved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending reviews count");
                return 0;
            }
        }

        /// <summary>
        /// Get statistics cards for primary metrics
        /// </summary>
        private static List<StatisticsCardViewModel> GetStatisticsCards(DashboardOverviewViewModel model)
        {
            return new List<StatisticsCardViewModel>
            {
                new StatisticsCardViewModel
                {
                    Title = "Total Users",
                    Value = model.TotalUsers.ToString("N0"),
                    Subtitle = $"▲ +{Math.Round(model.UsersTrend, 1)}% from last month",
                    IconClass = "fas fa-users",
                    ColorClass = "primary",
                    Trend = model.UsersTrend,
                    IsTrendPositive = model.UsersTrend > 0
                },
                new StatisticsCardViewModel
                {
                    Title = "Active Trainers",
                    Value = model.TotalTrainers.ToString("N0"),
                    Subtitle = $"▲ +{Math.Round(model.TrainersTrend, 1)}% from last month",
                    IconClass = "fas fa-dumbbell",
                    ColorClass = "success",
                    Trend = model.TrainersTrend,
                    IsTrendPositive = model.TrainersTrend > 0
                },
                new StatisticsCardViewModel
                {
                    Title = "Active Subscriptions",
                    Value = model.ActiveSubscriptions.ToString("N0"),
                    Subtitle = $"▲ +{Math.Round(model.SubscriptionsTrend, 1)}% from last month",
                    IconClass = "fas fa-check-circle",
                    ColorClass = "success",
                    Trend = model.SubscriptionsTrend,
                    IsTrendPositive = model.SubscriptionsTrend > 0
                },
                new StatisticsCardViewModel
                {
                    Title = "Total Revenue",
                    Value = $"EGP {model.TotalRevenue:N0}",
                    Subtitle = $"▲ +{Math.Round(model.RevenueTrend, 1)}% from last month",
                    IconClass = "fas fa-money-bill-wave",
                    ColorClass = "primary",
                    Trend = model.RevenueTrend,
                    IsTrendPositive = model.RevenueTrend > 0
                }
            };
        }

        /// <summary>
        /// Get top trainers by rating and client count
        /// </summary>
        private List<TopTrainerViewModel> GetTopTrainers(IEnumerable<TrainerProfile> trainers)
        {
            try
            {
                return trainers
                    .OrderByDescending(t => t.RatingAverage)
                    .ThenByDescending(t => t.TotalClients)
                    .Take(5)
                    .Select(t => new TopTrainerViewModel
                    {
                        TrainerId = t.Id,
                        Name = t.User?.FullName ?? t.Handle,
                        Handle = t.Handle,
                        ClientCount = t.TotalClients,
                        Rating = t.RatingAverage,
                        YearsExperience = t.YearsExperience
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top trainers");
                return new List<TopTrainerViewModel>();
            }
        }

        /// <summary>
        /// Get recent activities from subscriptions and payments
        /// </summary>
        private List<RecentActivityViewModel> GetRecentActivities(
            List<Subscription> subscriptions,
            List<DomainPayment> payments)
        {
            try
            {
                var activities = new List<RecentActivityViewModel>();

                // Add recent subscriptions (new users)
                foreach (var sub in subscriptions
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(3))
                {
                    if (sub.Client != null)
                    {
                        activities.Add(new RecentActivityViewModel
                        {
                            Title = "New Subscription",
                            Description = $"{sub.Client.FullName} subscribed to {sub.Package?.Name ?? "a package"}",
                            IconClass = "fas fa-check-circle",
                            ColorClass = "success",
                            Timestamp = sub.CreatedAt
                        });
                    }
                }

                // Add completed payments
                foreach (var p in payments
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .OrderByDescending(p => p.PaidAt)
                    .Take(3))
                {
                    if (p.Subscription?.Client != null)
                    {
                        activities.Add(new RecentActivityViewModel
                        {
                            Title = "Payment Completed",
                            Description = $"Payment of EGP {p.Amount:N0} from {p.Subscription.Client.FullName}",
                            IconClass = "fas fa-money-bill-wave",
                            ColorClass = "primary",
                            Timestamp = p.PaidAt ?? p.CreatedAt
                        });
                    }
                }

                // Add failed payments
                foreach (var p in payments
                    .Where(p => p.Status == PaymentStatus.Failed)
                    .OrderByDescending(p => p.FailedAt)
                    .Take(2))
                {
                    if (p.Subscription?.Client != null)
                    {
                        activities.Add(new RecentActivityViewModel
                        {
                            Title = "Payment Failed",
                            Description = $"Payment failed for {p.Subscription.Client.FullName}: {p.FailureReason}",
                            IconClass = "fas fa-exclamation-circle",
                            ColorClass = "danger",
                            Timestamp = p.FailedAt ?? p.CreatedAt
                        });
                    }
                }

                return activities
                    .OrderByDescending(a => a.Timestamp)
                    .Take(5)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activities");
                return new List<RecentActivityViewModel>();
            }
        }

        /// <summary>
        /// Get revenue analytics for specified period
        /// Aggregates daily revenue data from payments
        /// </summary>
        public async Task<ChartDataViewModel> GetRevenueChartDataAsync(int days = 30)
        {
            try
            {
                if (days < 7 || days > 365)
                    days = 30;

                var chartData = new ChartDataViewModel();
                var endDate = DateTime.UtcNow.Date;
                var startDate = endDate.AddDays(-days);

                // Get payments for the period
                var spec = new DashboardPaymentAnalyticsSpecs(startDate, endDate.AddDays(1));
                var payments = await _unitOfWork
                    .Repository<DomainPayment>()
                    .GetAllWithSpecsAsync(spec);

                // Aggregate by day
                var dailyRevenue = payments
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .GroupBy(p => p.PaidAt?.Date ?? p.CreatedAt.Date)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => (decimal)g.Sum(p => p.Amount));

                // Generate labels and data
                for (int i = 0; i < days; i++)
                {
                    var date = startDate.AddDays(i);
                    chartData.Labels.Add(date.ToString("MMM dd"));
                    var value = dailyRevenue.ContainsKey(date) ? dailyRevenue[date] : 0m;
                    chartData.Data.Add(value);
                }

                chartData.Datasets.Add(new ChartDatasetViewModel
                {
                    Label = "Revenue (EGP)",
                    Data = chartData.Data.OfType<decimal>().ToList(),
                    BackgroundColor = "rgba(37, 99, 235, 0.1)",
                    BorderColor = "#2563EB",
                    BorderWidth = 2,
                    Tension = 0.4
                });

                return chartData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue chart data");
                throw;
            }
        }

        /// <summary>
        /// Get subscription growth analytics
        /// </summary>
        public async Task<ChartDataViewModel> GetSubscriptionGrowthChartDataAsync(int days = 30)
        {
            try
            {
                if (days < 7 || days > 365)
                    days = 30;

                var chartData = new ChartDataViewModel();
                var endDate = DateTime.UtcNow.Date;
                var startDate = endDate.AddDays(-days);

                // Get subscriptions for the period
                var spec = new DashboardSubscriptionAnalyticsSpecs(startDate, endDate.AddDays(1));
                var subscriptions = await _unitOfWork
                    .Repository<Subscription>()
                    .GetAllWithSpecsAsync(spec);

                // Aggregate by day
                var dailyCounts = subscriptions
                    .GroupBy(s => s.CreatedAt.Date)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => (decimal)g.Count());

                // Generate labels and data
                for (int i = 0; i < days; i++)
                {
                    var date = startDate.AddDays(i);
                    chartData.Labels.Add(date.ToString("MMM dd"));
                    var value = dailyCounts.ContainsKey(date) ? dailyCounts[date] : 0m;
                    chartData.Data.Add(value);
                }

                chartData.Datasets.Add(new ChartDatasetViewModel
                {
                    Label = "New Subscriptions",
                    Data = chartData.Data.OfType<decimal>().ToList(),
                    BackgroundColor = "rgba(34, 197, 94, 0.8)",
                    BorderColor = "#22C55E",
                    BorderWidth = 2
                });

                return chartData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription growth chart data");
                throw;
            }
        }

        /// <summary>
        /// Get user distribution data
        /// </summary>
        public async Task<ChartDataViewModel> GetUserDistributionChartDataAsync()
        {
            try
            {
                var subscriptions = await _unitOfWork.Repository<Subscription>().GetAllAsync();
                var trainers = await _unitOfWork.Repository<TrainerProfile>().GetAllAsync();

                var clients = subscriptions.Select(s => s.ClientId).Distinct().Count();
                var activeTrainers = trainers.Count(t => t.IsVerified);
                var admins = 5; // Mock admin count

                return new ChartDataViewModel
                {
                    Labels = new List<string> { "Clients", "Trainers", "Admins" },
                    Data = new List<object> { clients, activeTrainers, admins },
                    Datasets = new List<ChartDatasetViewModel>
                    {
                        new ChartDatasetViewModel
                        {
                            Label = "User Distribution",
                            Data = new List<decimal> { clients, activeTrainers, admins }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user distribution data");
                throw;
            }
        }

        /// <summary>
        /// Get payment status breakdown
        /// </summary>
        public async Task<ChartDataViewModel> GetPaymentStatusChartDataAsync()
        {
            try
            {
                var spec = new DashboardPaymentAnalyticsSpecs();
                var payments = await _unitOfWork
                    .Repository<DomainPayment>()
                    .GetAllWithSpecsAsync(spec);

                var completed = (decimal)payments.Count(p => p.Status == PaymentStatus.Completed);
                var failed = (decimal)payments.Count(p => p.Status == PaymentStatus.Failed);
                var pending = (decimal)payments.Count(p => p.Status == PaymentStatus.Pending);

                return new ChartDataViewModel
                {
                    Labels = new List<string> { "Completed", "Failed", "Pending" },
                    Data = new List<object> { completed, failed, pending },
                    Datasets = new List<ChartDatasetViewModel>
                    {
                        new ChartDatasetViewModel
                        {
                            Label = "Payment Status",
                            Data = new List<decimal> { completed, failed, pending }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status chart data");
                throw;
            }
        }

        /// <summary>
        /// Get subscription status breakdown
        /// </summary>
        public async Task<ChartDataViewModel> GetSubscriptionStatusChartDataAsync()
        {
            try
            {
                var spec = new DashboardSubscriptionAnalyticsSpecs();
                var subscriptions = await _unitOfWork
                    .Repository<Subscription>()
                    .GetAllWithSpecsAsync(spec);

                var active = (decimal)subscriptions.Count(s => s.Status == SubscriptionStatus.Active);
                var unpaid = (decimal)subscriptions.Count(s => s.Status == SubscriptionStatus.Unpaid);
                var canceled = (decimal)subscriptions.Count(s => s.Status == SubscriptionStatus.Canceled);

                return new ChartDataViewModel
                {
                    Labels = new List<string> { "Active", "Unpaid", "Canceled" },
                    Data = new List<object> { active, unpaid, canceled },
                    Datasets = new List<ChartDatasetViewModel>
                    {
                        new ChartDatasetViewModel
                        {
                            Label = "Subscription Status",
                            Data = new List<decimal> { active, unpaid, canceled }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription status chart data");
                throw;
            }
        }
    }
}