using Gymunity.Application.Common;
using Gymunity.Application.DTOs.User.Payment;
using Gymunity.Domain.Enums;

namespace Gymunity.Application.Contracts.Services.Client
{
    public interface IPaymentService
    {
        Task<ServiceResult<PaymentResponse>> InitiatePaymentAsync(
            string clientId,
            InitiatePaymentRequest request);
        Task ConfirmPaymentAsync(int paymentId, string captureId);
        Task FailPaymentAsync(int paymentId, string reason);
        Task<ServiceResult<PaymentHistoryResponse>> GetPaymentHistoryAsync(string clientId, PaymentStatus? status);
        Task<ServiceResult<PaymentResponse>> GetPaymentByIdAsync(
                 int paymentId,
                 string clientId);
    }
}