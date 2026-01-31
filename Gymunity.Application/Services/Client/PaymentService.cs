using AutoMapper;
using Gymunity.Application.Common;
using Gymunity.Application.Contracts.ExternalServices;
using Gymunity.Application.Contracts.Services.Client;
using Gymunity.Application.DTOs.User.Payment;
using Gymunity.Application.Specifications.Payment;
using Gymunity.Application.Specifications.Subscriptions;
using Gymunity.Domain;
using Gymunity.Domain.Entities;
using Gymunity.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gymunity.Application.Services.Client
{
    public class PaymentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPayPalService paypalService,
        IStripePaymentService stripePaymentService,
        ILogger<PaymentService> logger,
        IConfiguration configuration) : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly IPayPalService _paypalService = paypalService;
        private readonly IStripePaymentService _stripePaymentService = stripePaymentService;
        private readonly ILogger<PaymentService> _logger = logger;
        private readonly IConfiguration _configuration = configuration;

        // Get frontend base URL from configuration
        private readonly string _frontendBaseUrl = configuration["FrontendOrigins:Hosted"]
                    ?? configuration["FrontendOrigins:Local"] ?? "https://localhost:4200";

        // ===============================
        // Initiate Payment
        // ===============================
        public async Task<ServiceResult<PaymentResponse>> InitiatePaymentAsync(
            string clientId,
            InitiatePaymentRequest request)
        {
            try
            {
                // 1️⃣ Validate subscription
                var subSpec = new ClientSubscriptionByIdSpecs(
                    request.SubscriptionId, clientId);

                var subscription = await _unitOfWork
                    .Repository<Subscription>()
                    .GetWithSpecsAsync(subSpec);


                if (subscription == null)
                    return ServiceResult<PaymentResponse>
                        .Failure("Subscription not found");

                // ✅ ENHANCED: Only allow Unpaid or PastDue subscriptions
                if (subscription.Status != SubscriptionStatus.Unpaid && 
                    subscription.Status != SubscriptionStatus.PastDue)
                {
                    return ServiceResult<PaymentResponse>
                        .Failure($"Cannot initiate payment for subscription with status: {subscription.Status}. Only 'Unpaid' or 'PastDue' subscriptions can be paid.");
                }

                // 2️⃣ Check existing pending payment
                var existingPayments = await _unitOfWork
                    .Repository<Payment>()
                    .GetAllWithSpecsAsync(
                        new PaymentBySubscriptionSpecs(subscription.Id, clientId));

                // ✅ FIXED: Check for BOTH Pending AND Processing payments
                var pendingPayment = existingPayments.FirstOrDefault(p => 
                    p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing);

                if (pendingPayment != null)
                {
                    // ✅ ENHANCED: Reload with relations for complete response
                    var existingPaymentWithRelations = await _unitOfWork
                        .Repository<Payment>()
                        .GetWithSpecsAsync(new PaymentByIdSpecs(pendingPayment.Id, clientId));

                    if (existingPaymentWithRelations != null && existingPaymentWithRelations.Method == request.PaymentMethod)
                    {
                        var pendingResponse = _mapper.Map<PaymentResponse>(existingPaymentWithRelations);
                        
                        // ✅ ENHANCED: Set appropriate payment URL based on payment method
                        if (existingPaymentWithRelations.Method == PaymentMethod.PayPal)
                        {
                            if (!string.IsNullOrEmpty(subscription.PayPalApprovalUrl))
                                pendingResponse.PaymentUrl = subscription.PayPalApprovalUrl;
                        }
                        else if (existingPaymentWithRelations.Method == PaymentMethod.Stripe)
                        {
                            if (!string.IsNullOrEmpty(subscription.StripeCheckoutUrl))
                                pendingResponse.PaymentUrl = subscription.StripeCheckoutUrl;  // ✅ Use the actual URL, not ID
                        }
                        
                        _logger.LogInformation(
                            "Returning existing pending payment {PaymentId} for subscription {SubscriptionId}",
                            pendingPayment.Id,
                            subscription.Id);
                        
                        return ServiceResult<PaymentResponse>.Success(pendingResponse);
                    }
                }

                // 3️⃣ Initiate gateway BEFORE creating payment record
                string? paymentUrl = null;
                string? gatewayOrderId = null;
                string? stripeSessionId = null;

                if (request.PaymentMethod == PaymentMethod.PayPal)
                {
                    var returnUrl = request.ReturnUrl ??
                        $"{_frontendBaseUrl}/payment/return?subscriptionId={subscription.Id}";

                    var cancelUrl = 
                        $"{_frontendBaseUrl}/payment/cancel?subscriptionId={subscription.Id}";

                    // ✅ Call PayPal FIRST
                    var result = await _paypalService.CreateOrderAsync(
                        subscription,
                        returnUrl,
                        cancelUrl);

                    if (!result.Success)
                    {
                        _logger.LogWarning(
                            "PayPal order creation failed for subscription {SubscriptionId}: {ErrorMessage}",
                            subscription.Id,
                            result.ErrorMessage);

                        return ServiceResult<PaymentResponse>
                            .Failure($"Failed to process payment: {result.ErrorMessage ?? "Payment gateway error"}");
                    }

                    gatewayOrderId = result.OrderId;
                    paymentUrl = result.ApprovalUrl;

                    // Store on subscription
                    subscription.PayPalOrderId = result.OrderId;
                    subscription.PayPalApprovalUrl = result.ApprovalUrl;
                }
                else if (request.PaymentMethod == PaymentMethod.Stripe)
                {
                    var returnUrl = request.ReturnUrl ??
                        $"{_frontendBaseUrl}/payment/return?subscriptionId={subscription.Id}";

                    var cancelUrl =
                        $"{_frontendBaseUrl}/payment/cancel?subscriptionId={subscription.Id}";

                    // ✅ Call Stripe FIRST
                    var result = await _stripePaymentService.CreateCheckoutSessionAsync(
                        subscription,
                        returnUrl,
                        cancelUrl);

                    if (!result.Success)
                    {
                        _logger.LogWarning(
                            "Stripe checkout session creation failed for subscription {SubscriptionId}: {ErrorMessage}",
                            subscription.Id,
                            result.ErrorMessage);

                        return ServiceResult<PaymentResponse>
                            .Failure($"Failed to process payment: {result.ErrorMessage ?? "Payment gateway error"}");
                    }

                    stripeSessionId = result.SessionId;
                    paymentUrl = result.CheckoutUrl;

                    // Store on subscription
                    subscription.StripePaymentIntentId = result.SessionId;
                    subscription.StripeCheckoutUrl = result.CheckoutUrl;  // ✅ Store the actual URL
                }
                else
                {
                    return ServiceResult<PaymentResponse>
                        .Failure("Payment method not supported");
                }

                // 4️⃣ NOW create payment record (AFTER gateway succeeds)
                var payment = new Payment
                {
                    SubscriptionId = subscription.Id,
                    ClientId = clientId,
                    Amount = subscription.AmountPaid,
                    Currency = subscription.Currency ?? "USD",
                    Method = request.PaymentMethod,
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                payment.CalculateFees(subscription.PlatformFeePercentage);

                // ✅ Store gateway IDs on payment record
                if (request.PaymentMethod == PaymentMethod.PayPal)
                {
                    payment.PayPalOrderId = gatewayOrderId;
                }
                else if (request.PaymentMethod == PaymentMethod.Stripe)
                {
                    payment.StripeSessionId = stripeSessionId;
                }

                _unitOfWork.Repository<Payment>().Add(payment);
                _unitOfWork.Repository<Subscription>().Update(subscription);
                await _unitOfWork.CompleteAsync();

                // 5️⃣ Reload payment with all relations for complete response
                var completePayment = await _unitOfWork
                    .Repository<Payment>()
                    .GetWithSpecsAsync(new PaymentByIdSpecs(payment.Id, clientId));

                if (completePayment == null)
                {
                    _logger.LogError("Failed to retrieve created payment {PaymentId}", payment.Id);
                    return ServiceResult<PaymentResponse>.Failure("Failed to retrieve created payment");
                }

                var response = _mapper.Map<PaymentResponse>(completePayment);
                response.PaymentUrl = paymentUrl;

                _logger.LogInformation(
                    "Payment {PaymentId} initiated successfully for subscription {SubscriptionId}",
                    payment.Id,
                    subscription.Id);

                return ServiceResult<PaymentResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment initiation failed for client {ClientId}", clientId);
                return ServiceResult<PaymentResponse>
                    .Failure("Failed to initiate payment");
            }
        }

        // ===============================
        // Confirm Payment (after PayPal/Stripe return)
        // ===============================
        public async Task ConfirmPaymentAsync(int paymentId, string captureId)
        {
            var payment = await _unitOfWork
                .Repository<Payment>()
                .GetByIdAsync(paymentId);

            if (payment == null) 
            {
                _logger.LogWarning("Payment {PaymentId} not found for confirmation", paymentId);
                return;
            }

            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            
            if (payment.Method == PaymentMethod.PayPal)
            {
                payment.PayPalCaptureId = captureId;
            }

            var subscription = await _unitOfWork
                .Repository<Subscription>()
                .GetByIdAsync(payment.SubscriptionId);

            if (subscription != null)
            {
                subscription.Status = SubscriptionStatus.Active;
                
                // ✅ Update subscription with payment intent if Stripe
                if (payment.Method == PaymentMethod.Stripe)
                {
                    subscription.StripePaymentIntentId = captureId;
                }

                _unitOfWork.Repository<Subscription>().Update(subscription);
            }

            _unitOfWork.Repository<Payment>().Update(payment);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Payment {PaymentId} confirmed with capture ID {CaptureId}",
                paymentId,
                captureId);
        }

        // ===============================
        // Fail Payment
        // ===============================
        public async Task FailPaymentAsync(int paymentId, string reason)
        {
            var payment = await _unitOfWork
                .Repository<Payment>()
                .GetByIdAsync(paymentId);

            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found for failure", paymentId);
                return;
            }

            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = reason;
            payment.FailedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Payment>().Update(payment);
            await _unitOfWork.CompleteAsync();

            _logger.LogWarning(
                "Payment {PaymentId} failed with reason: {Reason}",
                paymentId,
                reason);
        }

        // ===============================
        // Get Payment History
        // ===============================
        public async Task<ServiceResult<PaymentHistoryResponse>>
            GetPaymentHistoryAsync(string clientId, PaymentStatus? status)
        {
            var payments = await _unitOfWork
                .Repository<Payment>()
                .GetAllWithSpecsAsync(
                    new ClientPaymentsSpecs(clientId, status));

            var list = payments
                .Select(p => _mapper.Map<PaymentResponse>(p))
                .ToList();

            return ServiceResult<PaymentHistoryResponse>.Success(
                new PaymentHistoryResponse
                {
                    TotalPayments = list.Count,
                    TotalAmount = list.Sum(p => p.Amount),
                    Payments = list
                });
        }

        public async Task<ServiceResult<PaymentResponse>> GetPaymentByIdAsync(
            int paymentId,
            string clientId)
        {
            var payment = await _unitOfWork
                .Repository<Payment>()
                .GetWithSpecsAsync(new PaymentByIdSpecs(paymentId, clientId));

            if (payment == null)
                return ServiceResult<PaymentResponse>.Failure("Payment not found");

            var response = _mapper.Map<PaymentResponse>(payment);
            return ServiceResult<PaymentResponse>.Success(response);
        }
    }
}
