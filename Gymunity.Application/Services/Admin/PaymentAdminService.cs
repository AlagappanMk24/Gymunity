using AutoMapper;
using Gymunity.Application.Contracts.Services.Admin;
using Gymunity.Application.DTOs.User.Payment;
using Gymunity.Application.Specifications.Admin;
using Gymunity.Domain;
using Gymunity.Domain.Entities;
using Gymunity.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Gymunity.Application.Services.Admin
{
    /// <summary>
    /// Admin service for managing payments, refunds, and payment analytics
    /// Events are raised when important operations complete for notification handlers to subscribe to
    /// </summary>
    public class PaymentAdminService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PaymentAdminService> logger) : IPaymentAdminService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<PaymentAdminService> _logger = logger;

        // ✅ Observable events for notification handlers
        public event Func<int, Payment, Task>? RefundCompletedAsync;
        public event Func<int, Payment, Task>? PaymentMarkedAsFailedAsync;

        /// <summary>
        /// Get all payments with optional filtering and pagination
        /// </summary>
        public async Task<IEnumerable<PaymentResponse>> GetAllPaymentsAsync(
            PaymentFilterSpecs specs)
        {
            try
            {
                var payments = await _unitOfWork
                    .Repository<Payment>()
                    .GetAllWithSpecsAsync(specs);

                return _mapper.Map<IEnumerable<PaymentResponse>>(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments with specs");
                throw;
            }
        }

        /// <summary>
        /// Get payment by ID
        /// </summary>
        public async Task<PaymentResponse?> GetPaymentByIdAsync(int paymentId)
        {
            try
            {
                var payment = await _unitOfWork
                    .Repository<Payment>()
                    .GetByIdAsync(paymentId);

                if (payment == null)
                    return null;

                return _mapper.Map<PaymentResponse>(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment with ID {PaymentId}", paymentId);
                throw;
            }
        }

        /// <summary>
        /// Get count of payments matching specification
        /// </summary>
        public async Task<int> GetPaymentCountAsync(PaymentFilterSpecs specs)
        {
            try
            {
                return await _unitOfWork
                    .Repository<Payment>()
                    .GetCountWithspecsAsync(specs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment count");
                throw;
            }
        }

        /// <summary>
        /// Get failed payments
        /// </summary>
        public async Task<IEnumerable<PaymentResponse>> GetFailedPaymentsAsync(
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var specs = new PaymentFilterSpecs(
                    status: PaymentStatus.Failed,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                var payments = await _unitOfWork
                    .Repository<Payment>()
                    .GetAllWithSpecsAsync(specs);

                return _mapper.Map<IEnumerable<PaymentResponse>>(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving failed payments");
                throw;
            }
        }

        /// <summary>
        /// Get completed payments
        /// </summary>
        public async Task<IEnumerable<PaymentResponse>> GetCompletedPaymentsAsync(
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var specs = new PaymentFilterSpecs(
                    status: PaymentStatus.Completed,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                var payments = await _unitOfWork
                    .Repository<Payment>()
                    .GetAllWithSpecsAsync(specs);

                return _mapper.Map<IEnumerable<PaymentResponse>>(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving completed payments");
                throw;
            }
        }

        /// <summary>
        /// Get pending payments
        /// </summary>
        public async Task<IEnumerable<PaymentResponse>> GetPendingPaymentsAsync(
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var specs = new PaymentFilterSpecs(
                    status: PaymentStatus.Pending,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                var payments = await _unitOfWork
                    .Repository<Payment>()
                    .GetAllWithSpecsAsync(specs);

                return _mapper.Map<IEnumerable<PaymentResponse>>(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending payments");
                throw;
            }
        }

        /// <summary>
        /// Get refunded payments
        /// </summary>
        public async Task<IEnumerable<PaymentResponse>> GetRefundedPaymentsAsync(
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var specs = new PaymentFilterSpecs(
                    status: PaymentStatus.Refunded,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                var payments = await _unitOfWork
                    .Repository<Payment>()
                    .GetAllWithSpecsAsync(specs);

                return _mapper.Map<IEnumerable<PaymentResponse>>(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refunded payments");
                throw;
            }
        }

        /// <summary>
        /// Get revenue for a date range
        /// </summary>
        public async Task<decimal> GetRevenueAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var specs = new PaymentFilterSpecs(
                    status: PaymentStatus.Completed,
                    startDate: startDate,
                    endDate: endDate);

                var payments = await _unitOfWork
                    .Repository<Payment>()
                    .GetAllWithSpecsAsync(specs);

                return payments.Sum(p => p.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating revenue for date range");
                throw;
            }
        }

        /// <summary>
        /// Process refund for a payment
        /// Raises RefundCompletedAsync event after successful completion
        /// </summary>
        public async Task<bool> ProcessRefundAsync(int paymentId)
        {
            try
            {
                var payment = await _unitOfWork
                    .Repository<Payment>()
                    .GetByIdAsync(paymentId);

                if (payment == null)
                {
                    _logger.LogWarning("Payment with ID {PaymentId} not found for refund", paymentId);
                    return false;
                }

                if (payment.Status == PaymentStatus.Refunded)
                {
                    _logger.LogWarning("Payment {PaymentId} already refunded", paymentId);
                    return false;
                }

                payment.Status = PaymentStatus.Refunded;

                _unitOfWork.Repository<Payment>().Update(payment);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Payment {PaymentId} refunded successfully", paymentId);
                
                // ✅ Raise event for notification handlers
                if (RefundCompletedAsync != null)
                {
                    try
                    {
                        await RefundCompletedAsync(paymentId, payment);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Event notification failed for refund {PaymentId}", paymentId);
                        // Don't rethrow - refund already succeeded
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", paymentId);
                throw;
            }
        }

        /// <summary>
        /// Get total revenue (all completed payments)
        /// </summary>
        public async Task<decimal> GetTotalRevenueAsync()
        {
            try
            {
                var specs = new PaymentFilterSpecs(status: PaymentStatus.Completed);
                var payments = await _unitOfWork
                    .Repository<Payment>()
                    .GetAllWithSpecsAsync(specs);

                return payments.Sum(p => p.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total revenue");
                throw;
            }
        }

        /// <summary>
        /// Get count of failed payments
        /// </summary>
        public async Task<int> GetFailedPaymentCountAsync()
        {
            try
            {
                var specs = new PaymentFilterSpecs(status: PaymentStatus.Failed);
                return await _unitOfWork
                    .Repository<Payment>()
                    .GetCountWithspecsAsync(specs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting failed payment count");
                throw;
            }
        }

        /// <summary>
        /// Get payment details with client and subscription information using specification
        /// </summary>
        public async Task<PaymentResponse?> GetPaymentDetailsWithClientAsync(int paymentId)
        {
            try
            {
                var spec = new PaymentDetailWithClientSpecs(paymentId);
                var payments = await _unitOfWork
                    .Repository<Payment>()
                    .GetAllWithSpecsAsync(spec);

                var payment = payments.FirstOrDefault();
                if (payment == null)
                {
                    _logger.LogWarning("Payment with ID {PaymentId} not found", paymentId);
                    return null;
                }

                _logger.LogDebug("Retrieved payment {PaymentId} with client data", paymentId);
                return _mapper.Map<PaymentResponse>(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment details with client for ID {PaymentId}", paymentId);
                throw;
            }
        }

        /// <summary>
        /// Get all payments with advanced filtering using specification
        /// Supports multiple filter criteria for AJAX requests
        /// </summary>
        public async Task<IEnumerable<PaymentResponse>> GetPaymentsWithAdvancedFilterAsync(
            PaymentStatus? status = null,
            string? clientSearch = null,
            int? trainerProfileId = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var spec = new PaymentAdvancedFilterSpecs(
                    status: status,
                    clientSearch: clientSearch,
                    trainerProfileId: trainerProfileId,
                    minAmount: minAmount,
                    maxAmount: maxAmount,
                    startDate: startDate,
                    endDate: endDate,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                var payments = await _unitOfWork
                    .Repository<Payment>()
                    .GetAllWithSpecsAsync(spec);

                _logger.LogDebug("Retrieved {Count} payments with advanced filters", payments.Count());
                return _mapper.Map<IEnumerable<PaymentResponse>>(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments with advanced filters");
                throw;
            }
        }

        /// <summary>
        /// Get count of payments with advanced filtering
        /// </summary>
        public async Task<int> GetPaymentCountWithAdvancedFilterAsync(
            PaymentStatus? status = null,
            string? clientSearch = null,
            int? trainerProfileId = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var spec = new PaymentAdvancedFilterSpecs(
                    status: status,
                    clientSearch: clientSearch,
                    trainerProfileId: trainerProfileId,
                    minAmount: minAmount,
                    maxAmount: maxAmount,
                    startDate: startDate,
                    endDate: endDate);

                return await _unitOfWork
                    .Repository<Payment>()
                    .GetCountWithspecsAsync(spec);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment count with advanced filters");
                throw;
            }
        }

        /// <summary>
        /// Get revenue statistics for dashboard
        /// </summary>
        public async Task<(int totalPayments, decimal totalRevenue, int completedCount, int failedCount)> GetPaymentStatsAsync()
        {
            try
            {
                var totalSpecs = new PaymentFilterSpecs();
                var totalCount = await _unitOfWork
                    .Repository<Payment>()
                    .GetCountWithspecsAsync(totalSpecs);

                var completedSpecs = new PaymentFilterSpecs(status: PaymentStatus.Completed);
                var completedPayments = await _unitOfWork
                    .Repository<Payment>()
                    .GetAllWithSpecsAsync(completedSpecs);

                var totalRevenue = completedPayments.Sum(p => p.Amount);
                var completedCount = completedPayments.Count();

                var failedSpecs = new PaymentFilterSpecs(status: PaymentStatus.Failed);
                var failedCount = await _unitOfWork
                    .Repository<Payment>()
                    .GetCountWithspecsAsync(failedSpecs);

                _logger.LogDebug("Retrieved payment stats: Total={Total}, Revenue={Revenue}, Completed={Completed}, Failed={Failed}",
                    totalCount, totalRevenue, completedCount, failedCount);

                return (totalCount, totalRevenue, completedCount, failedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment statistics");
                throw;
            }
        }
    }
}
