using AutoMapper;
using Gymunity.Application.Common;
using Gymunity.Application.Contracts.ExternalServices;
using Gymunity.Application.Contracts.Services.Client;
using Gymunity.Application.DTOs.User.Subscriptions;
using Gymunity.Application.Specifications.Subscriptions;
using Gymunity.Domain;
using Gymunity.Domain.Entities;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ITI.Gymunity.FP.Application.Services
{
    public class SubscriptionService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IStripePaymentService stripePaymentService,
        IPayPalService paypalService,
        IConfiguration configuration,
        ILogger<SubscriptionService> logger) : ISubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly IStripePaymentService _stripePaymentService = stripePaymentService;
        private readonly IPayPalService _paypalService = paypalService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<SubscriptionService> _logger = logger;

        public async Task<bool> HasActiveSubscribtionToPackageAsync(
            string clientId,
            int packageId)
        {
            var spec = new ClientSubscriptionForPackageSpecs(clientId, packageId);
            var subscription = await _unitOfWork
                .Repository<Subscription>()
                .GetWithSpecsAsync(spec);
            return subscription != null;
        }

        /// <summary>
        /// Create subscription (UNPAID – waiting for payment)
        /// Now also creates Stripe Payment Intent
        /// </summary>
        public async Task<ServiceResult<SubscriptionResponse>> SubscribeAsync(
            string clientId,
            SubscribePackageRequest request)
        {
            try
            {
                // 1️⃣ Validate Package
                var package = await _unitOfWork
                    .Repository<Package>()
                    .GetByIdAsync(request.PackageId);

                if (package == null || !package.IsActive)
                {
                    return ServiceResult<SubscriptionResponse>.Failure(
                        "Package not found or inactive");
                }

                // 2️⃣ Prevent duplicate subscription
                var existingSpec =
                    new ClientSubscriptionForPackageSpecs(
                        clientId, request.PackageId);

                var subscription = await _unitOfWork
                    .Repository<Subscription>()
                    .GetWithSpecsAsync(existingSpec);

                if (subscription != null && subscription.Status == SubscriptionStatus.Active)
                {
                    return ServiceResult<SubscriptionResponse>.Failure(
                        "You already subscribed to this package");
                }

                // 3️⃣ Calculate amount & period
                decimal amount = request.IsAnnual
                    ? package.PriceYearly ?? package.PriceMonthly * 12
                    : package.PriceMonthly;

                DateTime periodEnd = request.IsAnnual
                    ? DateTime.UtcNow.AddYears(1)
                    : DateTime.UtcNow.AddMonths(1);


                // 4️⃣ Create Subscription (UNPAID)
                if (subscription != null)
                {
                    // Update existing unpaid subscription
                    subscription.Status = SubscriptionStatus.Unpaid;
                    subscription.StartDate = DateTime.UtcNow;
                    subscription.CurrentPeriodEnd = periodEnd;
                    subscription.AmountPaid = amount;
                    subscription.IsAnnual = request.IsAnnual;
                    _unitOfWork.Repository<Subscription>().Update(subscription);
                }
                else
                {
                    subscription = new Subscription
                    {
                        ClientId = clientId,
                        PackageId = request.PackageId,
                        Status = SubscriptionStatus.Unpaid,
                        StartDate = DateTime.UtcNow,
                        CurrentPeriodEnd = periodEnd,
                        AmountPaid = amount,
                        Currency = package.Currency ?? "USD",
                        PlatformFeePercentage = 15m,
                        IsAnnual = request.IsAnnual
                    };
                    _unitOfWork.Repository<Subscription>().Add(subscription);
                }


                await _unitOfWork.CompleteAsync();

                // 5️⃣ Reload subscription WITH includes (IMPORTANT)
                var createdSubscription = await _unitOfWork
                    .Repository<Subscription>()
                    .GetWithSpecsAsync(
                        new ClientSubscriptionByIdSpecs(subscription.Id, clientId));

                if (createdSubscription == null)
                {
                    return ServiceResult<SubscriptionResponse>.Failure(
                        "Failed to load created subscription");
                }

                // 6️⃣ Create Stripe Checkout Session ✅
                if (request.PaymentMethod == PaymentMethod.Stripe)
                {
                    var returnUrl = request.ReturnUrl ??
                        $"{_configuration["BaseApiUrl"]}/api/payment/stripe/return";

                    var cancelUrl =
                        $"{_configuration["BaseApiUrl"]}/api/payment/stripe/cancel";

                    var stripeResult = await _stripePaymentService
                        .CreateCheckoutSessionAsync(createdSubscription, returnUrl, cancelUrl);

                    if (!stripeResult.Success)
                    {
                        _logger.LogWarning(
                            "Failed to create Stripe checkout session for subscription {SubscriptionId}: {Error}",
                            createdSubscription.Id,
                            stripeResult.ErrorMessage);

                        // Continue anyway - client can retry payment
                    }
                    else
                    {
                        // ✅ Store session ID on subscription
                        createdSubscription.StripePaymentIntentId = stripeResult.SessionId;

                        _unitOfWork.Repository<Subscription>().Update(createdSubscription);
                        await _unitOfWork.CompleteAsync();

                        _logger.LogInformation(
                            "Stripe checkout session created for subscription {SubscriptionId}: {SessionId}",
                            createdSubscription.Id,
                            stripeResult.SessionId);
                    }
                }
                else if (request.PaymentMethod == PaymentMethod.PayPal)
                {
                    // 7️⃣ Create PayPal Order
                    var returnUrl = request.ReturnUrl ??
                        $"{_configuration["BaseApiUrl"]}/api/payment/paypal/return";

                    var cancelUrl =
                        $"{_configuration["BaseApiUrl"]}/api/payment/paypal/cancel";

                    var paypalResult = await _paypalService.CreateOrderAsync(
                        createdSubscription, returnUrl, cancelUrl);

                    if (!paypalResult.Success)
                    {
                        _logger.LogWarning(
                            "Failed to create PayPal order for subscription {SubscriptionId}: {Error}",
                            createdSubscription.Id,
                            paypalResult.ErrorMessage);

                        return ServiceResult<SubscriptionResponse>.Failure("Failed to create PayPal order");
                    }

                    // ✅ Store PayPal order details on subscription
                    createdSubscription.PayPalOrderId = paypalResult.OrderId;
                    createdSubscription.PayPalApprovalUrl = paypalResult.ApprovalUrl;

                    _unitOfWork.Repository<Subscription>().Update(createdSubscription);
                    await _unitOfWork.CompleteAsync();

                    _logger.LogInformation(
                        "PayPal order created for subscription {SubscriptionId}: {OrderId}",
                        createdSubscription.Id,
                        paypalResult.OrderId);
                }

                // 8️⃣ Map safely
                var response = _mapper.Map<SubscriptionResponse>(createdSubscription);

                _logger.LogInformation(
                    "Client {ClientId} subscribed to package {PackageId} successfully",
                    clientId, request.PackageId);

                return ServiceResult<SubscriptionResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error subscribing client {ClientId} to package {PackageId}",
                    clientId, request.PackageId);

                return ServiceResult<SubscriptionResponse>.Failure(
                    "Failed to create subscription");
            }
        }

        /// <summary>
        /// Get all client subscriptions with filtering
        /// </summary>
        public async Task<ServiceResult<SubscriptionListResponse>> GetClientSubscriptionsAsync(
            string clientId,
            SubscriptionStatus? status = null)
        {
            try
            {
                var spec = new ClientSubscriptionsSpecs(clientId, status);

                var subscriptions = await _unitOfWork
                    .Repository<Subscription>()
                    .GetAllWithSpecsAsync(spec);

                var response = new SubscriptionListResponse
                {
                    TotalSubscriptions = subscriptions.Count(),
                    ActiveSubscriptions = subscriptions.Count(s => s.Status == SubscriptionStatus.Active),
                    Subscriptions = _mapper.Map<List<SubscriptionResponse>>(subscriptions)
                };

                return ServiceResult<SubscriptionListResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscriptions for client {ClientId}", clientId);
                return ServiceResult<SubscriptionListResponse>.Failure("Failed to retrieve subscriptions");
            }
        }

        /// <summary>
        /// Get single subscription by ID
        /// </summary>
        public async Task<ServiceResult<SubscriptionResponse>> GetSubscriptionByIdAsync(
            int subscriptionId,
            string clientId)
        {
            try
            {
                var spec = new ClientSubscriptionByIdSpecs(subscriptionId, clientId);

                var subscription = await _unitOfWork
                    .Repository<Subscription>()
                    .GetWithSpecsAsync(spec);

                if (subscription == null)
                {
                    return ServiceResult<SubscriptionResponse>.Failure("Subscription not found");
                }

                var response = _mapper.Map<SubscriptionResponse>(subscription);

                return ServiceResult<SubscriptionResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscription {SubscriptionId}", subscriptionId);
                return ServiceResult<SubscriptionResponse>.Failure("Failed to retrieve subscription");
            }
        }

        /// <summary>
        /// Cancel subscription (for client)
        /// </summary>
        public async Task<ServiceResult<bool>> CancelSubscriptionAsync(
            int subscriptionId,
            string clientId)
        {
            try
            {
                var spec = new ClientSubscriptionByIdSpecs(subscriptionId, clientId);

                var subscription = await _unitOfWork
                    .Repository<Subscription>()
                    .GetWithSpecsAsync(spec);

                if (subscription == null)
                {
                    return ServiceResult<bool>.Failure("Subscription not found");
                }

                if (subscription.Status == SubscriptionStatus.Canceled)
                {
                    return ServiceResult<bool>.Failure("Subscription already canceled");
                }

                subscription.Status = SubscriptionStatus.Canceled;
                subscription.CanceledAt = DateTime.UtcNow;

                _unitOfWork.Repository<Subscription>().Update(subscription);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation(
                    "Client {ClientId} canceled subscription {SubscriptionId}",
                    clientId, subscriptionId);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling subscription {SubscriptionId}", subscriptionId);
                return ServiceResult<bool>.Failure("Failed to cancel subscription");
            }
        }

        /// <summary>
        /// Mark subscription as PAID (called after payment success)
        /// </summary>
        public async Task<ServiceResult<SubscriptionResponse>> ActivateSubscriptionAsync(
            int subscriptionId,
            string paymentTransactionId)
        {
            try
            {
                var subscription = await _unitOfWork
                    .Repository<Subscription>()
                    .GetByIdAsync(subscriptionId);

                if (subscription == null)
                {
                    return ServiceResult<SubscriptionResponse>.Failure("Subscription not found");
                }

                if (subscription.Status != SubscriptionStatus.Unpaid)
                {
                    return ServiceResult<SubscriptionResponse>.Failure("Subscription is not in unpaid state");
                }

                subscription.Status = SubscriptionStatus.Active;
                subscription.PaymobTransactionId = paymentTransactionId;

                _unitOfWork.Repository<Subscription>().Update(subscription);
                await _unitOfWork.CompleteAsync();

                // Reload with includes
                var spec = new ClientSubscriptionByIdSpecs(subscriptionId, subscription.ClientId);
                var activatedSubscription = await _unitOfWork
                    .Repository<Subscription>()
                    .GetWithSpecsAsync(spec);

                var response = _mapper.Map<SubscriptionResponse>(activatedSubscription);

                _logger.LogInformation(
                    "Subscription {SubscriptionId} activated successfully",
                    subscriptionId);

                return ServiceResult<SubscriptionResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating subscription {SubscriptionId}", subscriptionId);
                return ServiceResult<SubscriptionResponse>.Failure("Failed to activate subscription");
            }
        }
    }
}