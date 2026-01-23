using Gymunity.Application.DTOs.User.Payment;
using Gymunity.Application.Specifications.Admin;
using Gymunity.Domain.Enums;

namespace Gymunity.Application.Contracts.Services.Admin
{
    public interface IPaymentAdminService
    {
        Task<IEnumerable<PaymentResponse>> GetAllPaymentsAsync(
           PaymentFilterSpecs specs);
        Task<PaymentResponse?> GetPaymentByIdAsync(int paymentId);
        Task<int> GetPaymentCountAsync(PaymentFilterSpecs specs);
        Task<IEnumerable<PaymentResponse>> GetFailedPaymentsAsync(
            int pageNumber = 1,
            int pageSize = 10);
        Task<IEnumerable<PaymentResponse>> GetCompletedPaymentsAsync(
            int pageNumber = 1,
            int pageSize = 10);
        Task<IEnumerable<PaymentResponse>> GetPendingPaymentsAsync(
            int pageNumber = 1,
            int pageSize = 10);
        Task<IEnumerable<PaymentResponse>> GetRefundedPaymentsAsync(
           int pageNumber = 1,
           int pageSize = 10);
        Task<decimal> GetRevenueAsync(DateTime startDate, DateTime endDate);
        Task<bool> ProcessRefundAsync(int paymentId);
        Task<decimal> GetTotalRevenueAsync();
        Task<int> GetFailedPaymentCountAsync();
        Task<PaymentResponse?> GetPaymentDetailsWithClientAsync(int paymentId);
        Task<IEnumerable<PaymentResponse>> GetPaymentsWithAdvancedFilterAsync(
            PaymentStatus? status = null,
            string? clientSearch = null,
            int? trainerProfileId = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 10);
        Task<int> GetPaymentCountWithAdvancedFilterAsync(
            PaymentStatus? status = null,
            string? clientSearch = null,
            int? trainerProfileId = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
        Task<(int totalPayments, decimal totalRevenue, int completedCount, int failedCount)> GetPaymentStatsAsync();
    }
}