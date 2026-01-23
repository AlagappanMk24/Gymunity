using AutoMapper;
using Gymunity.Application.Contracts.Services.Admin;
using Gymunity.Application.DTOs.User.Subscriptions;
using Gymunity.Application.Specifications.Admin;
using Gymunity.Domain;
using Gymunity.Domain.Entities;
using Gymunity.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ITI.Gymunity.FP.Application.Services.Admin
{
    /// <summary>
    /// Admin service for managing subscriptions and subscription lifecycle
    /// Events are raised when important operations complete for notification handlers to subscribe to
    /// </summary>
    public class SubscriptionAdminService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SubscriptionAdminService> logger) : ISubscriptionAdminService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<SubscriptionAdminService> _logger = logger;

        // ✅ Observable events for notification handlers
        public event Func<int, Subscription, Task>? SubscriptionCancelledByAdminAsync;
        public event Func<int, Subscription, Task>? SubscriptionCreatedAsync;

        /// <summary>
        /// Get all subscriptions with optional filtering and pagination
        /// </summary>
        public async Task<IEnumerable<SubscriptionResponse>> GetAllSubscriptionsAsync(
            SubscriptionFilterSpecs specs)
        {
            try
            {
                var subscriptions = await _unitOfWork
                    .Repository<Subscription>()
                    .GetAllWithSpecsAsync(specs);

                return _mapper.Map<IEnumerable<SubscriptionResponse>>(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscriptions with specs");
                throw;
            }
        }

        /// <summary>
        /// Get subscription by ID
        /// </summary>
        public async Task<SubscriptionResponse?> GetSubscriptionByIdAsync(int subscriptionId)
        {
            try
            {
                var subscription = await _unitOfWork
                    .Repository<Subscription>()
                    .GetByIdAsync(subscriptionId);

                if (subscription == null)
                    return null;

                return _mapper.Map<SubscriptionResponse>(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscription with ID {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        /// <summary>
        /// Get count of subscriptions matching specification
        /// </summary>
        public async Task<int> GetSubscriptionCountAsync(SubscriptionFilterSpecs specs)
        {
            try
            {
                return await _unitOfWork
                    .Repository<Subscription>()
                    .GetCountWithspecsAsync(specs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription count");
                throw;
            }
        }

        /// <summary>
        /// Get active subscriptions
        /// </summary>
        public async Task<IEnumerable<SubscriptionResponse>> GetActiveSubscriptionsAsync(
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var specs = new SubscriptionFilterSpecs(
                    status: SubscriptionStatus.Active,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                var subscriptions = await _unitOfWork
                    .Repository<Subscription>()
                    .GetAllWithSpecsAsync(specs);

                return _mapper.Map<IEnumerable<SubscriptionResponse>>(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active subscriptions");
                throw;
            }
        }

        /// <summary>
        /// Get inactive subscriptions
        /// </summary>
        public async Task<IEnumerable<SubscriptionResponse>> GetInactiveSubscriptionsAsync(
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var specs = new SubscriptionFilterSpecs(
                    status: SubscriptionStatus.Canceled,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                var subscriptions = await _unitOfWork
                    .Repository<Subscription>()
                    .GetAllWithSpecsAsync(specs);

                return _mapper.Map<IEnumerable<SubscriptionResponse>>(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inactive subscriptions");
                throw;
            }
        }

        /// <summary>
        /// Get unpaid subscriptions
        /// </summary>
        public async Task<IEnumerable<SubscriptionResponse>> GetUnpaidSubscriptionsAsync(
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var specs = new SubscriptionFilterSpecs(
                    status: SubscriptionStatus.Unpaid,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                var subscriptions = await _unitOfWork
                    .Repository<Subscription>()
                    .GetAllWithSpecsAsync(specs);

                return _mapper.Map<IEnumerable<SubscriptionResponse>>(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unpaid subscriptions");
                throw;
            }
        }

        /// <summary>
        /// Cancel subscription (admin action)
        /// Raises SubscriptionCancelledByAdminAsync event after successful completion
        /// </summary>
        public async Task<bool> CancelSubscriptionAsync(int subscriptionId, string reason = "")
        {
            try
            {
                var subscription = await _unitOfWork
                    .Repository<Subscription>()
                    .GetByIdAsync(subscriptionId);

                if (subscription == null)
                {
                    _logger.LogWarning("Subscription with ID {SubscriptionId} not found for cancellation", subscriptionId);
                    return false;
                }

                if (subscription.Status == SubscriptionStatus.Canceled)
                {
                    _logger.LogWarning("Subscription {SubscriptionId} already canceled", subscriptionId);
                    return false;
                }

                subscription.Status = SubscriptionStatus.Canceled;
                subscription.CanceledAt = DateTime.UtcNow;

                _unitOfWork.Repository<Subscription>().Update(subscription);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Subscription {SubscriptionId} canceled by admin. Reason: {Reason}", 
                    subscriptionId, reason);

                // ✅ Raise event for notification handlers
                if (SubscriptionCancelledByAdminAsync != null)
                {
                    try
                    {
                        await SubscriptionCancelledByAdminAsync(subscriptionId, subscription);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Event notification failed for subscription cancellation {SubscriptionId}", subscriptionId);
                        // Don't rethrow - cancellation already succeeded
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        /// <summary>
        /// Get total active subscriptions count
        /// </summary>
        public async Task<int> GetActiveSubscriptionCountAsync()
        {
            try
            {
                var specs = new SubscriptionFilterSpecs(status: SubscriptionStatus.Active);
                return await _unitOfWork
                    .Repository<Subscription>()
                    .GetCountWithspecsAsync(specs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active subscription count");
                throw;
            }
        }

        /// <summary>
        /// Get expiring soon subscriptions (within 7 days)
        /// </summary>
        public async Task<IEnumerable<SubscriptionResponse>> GetExpiringSoonSubscriptionsAsync()
        {
            try
            {
                var allSubscriptions = await _unitOfWork
                    .Repository<Subscription>()
                    .GetAllAsync();

                var expiringSubscriptions = allSubscriptions
                    .Where(s => s.Status == SubscriptionStatus.Active &&
                                s.CurrentPeriodEnd >= DateTime.UtcNow &&
                                s.CurrentPeriodEnd <= DateTime.UtcNow.AddDays(7))
                    .ToList();

                return _mapper.Map<IEnumerable<SubscriptionResponse>>(expiringSubscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expiring subscriptions");
                throw;
            }
        }

        /// <summary>
        /// Get subscription details with trainer information using specification
        /// </summary>
        public async Task<SubscriptionResponse?> GetSubscriptionDetailsWithTrainerAsync(int subscriptionId)
        {
            try
            {
                var spec = new SubscriptionDetailWithTrainerSpecs(subscriptionId);
                var subscriptions = await _unitOfWork
                    .Repository<Subscription>()
                    .GetAllWithSpecsAsync(spec);

                var subscription = subscriptions.FirstOrDefault();
                if (subscription == null)
                {
                    _logger.LogWarning("Subscription with ID {SubscriptionId} not found", subscriptionId);
                    return null;
                }

                _logger.LogDebug("Retrieved subscription {SubscriptionId} with trainer data", subscriptionId);
                return _mapper.Map<SubscriptionResponse>(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscription details with trainer for ID {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        /// <summary>
        /// Get all subscriptions with advanced filtering using specification
        /// Supports multiple filter criteria for AJAX requests
        /// </summary>
        public async Task<IEnumerable<SubscriptionResponse>> GetSubscriptionsWithAdvancedFilterAsync(
            SubscriptionStatus? status = null,
            int? trainerId = null,
            string? searchTerm = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var spec = new SubscriptionAdvancedFilterSpecs(
                    status: status,
                    trainerId: trainerId,
                    searchTerm: searchTerm,
                    startDate: startDate,
                    endDate: endDate,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                var subscriptions = await _unitOfWork
                    .Repository<Subscription>()
                    .GetAllWithSpecsAsync(spec);

                _logger.LogDebug("Retrieved {Count} subscriptions with advanced filters", subscriptions.Count());
                return _mapper.Map<IEnumerable<SubscriptionResponse>>(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscriptions with advanced filters");
                throw;
            }
        }

        /// <summary>
        /// Get count of subscriptions with advanced filtering
        /// </summary>
        public async Task<int> GetSubscriptionCountWithAdvancedFilterAsync(
            SubscriptionStatus? status = null,
            int? trainerId = null,
            string? searchTerm = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var spec = new SubscriptionAdvancedFilterSpecs(
                    status: status,
                    trainerId: trainerId,
                    searchTerm: searchTerm,
                    startDate: startDate,
                    endDate: endDate);

                return await _unitOfWork
                    .Repository<Subscription>()
                    .GetCountWithspecsAsync(spec);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription count with advanced filters");
                throw;
            }
        }

        /// <summary>
        /// Get subscription statistics for dashboard
        /// </summary>
        public async Task<(int activeCount, int unpaidCount, int canceledCount, decimal totalRevenue)> GetSubscriptionStatsAsync()
        {
            try
            {
                var activeCount = await GetActiveSubscriptionCountAsync();
                
                var unpaidSpecs = new SubscriptionFilterSpecs(status: SubscriptionStatus.Unpaid);
                var unpaidCount = await _unitOfWork
                    .Repository<Subscription>()
                    .GetCountWithspecsAsync(unpaidSpecs);

                var canceledSpecs = new SubscriptionFilterSpecs(status: SubscriptionStatus.Canceled);
                var canceledCount = await _unitOfWork
                    .Repository<Subscription>()
                    .GetCountWithspecsAsync(canceledSpecs);

                // Calculate total revenue from active subscriptions using specification
                var activeWithPaymentsSpecs = new ActiveSubscriptionsWithPaymentsSpecs();
                var activeSubscriptions = await _unitOfWork
                    .Repository<Subscription>()
                    .GetAllWithSpecsAsync(activeWithPaymentsSpecs);

                // Sum completed payment amounts from active subscriptions
                var totalRevenue = activeSubscriptions
                    .SelectMany(s => s.Payments ?? new List<Payment>())
                    .Where(p => p.Status == PaymentStatus.Completed && !p.IsDeleted)
                    .Sum(p => p.Amount);

                _logger.LogDebug("Retrieved subscription stats: Active={Active}, Unpaid={Unpaid}, Canceled={Canceled}, Revenue={Revenue}",
                    activeCount, unpaidCount, canceledCount, totalRevenue);

                return (activeCount, unpaidCount, canceledCount, totalRevenue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscription statistics");
                throw;
            }
        }
    }
}
