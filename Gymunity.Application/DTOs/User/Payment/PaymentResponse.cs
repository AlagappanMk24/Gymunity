using Gymunity.Domain.Enums;


namespace Gymunity.Application.DTOs.User.Payment
{
    public class PaymentResponse
    {
        public int Id { get; set; }
        public int SubscriptionId { get; set; }
        
        // Client Information
        public string ClientId { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public string ClientEmail { get; set; } = null!;
        
        // Subscription Information
        public int PackageId { get; set; }
        public string PackageName { get; set; } = null!;
        public SubscriptionStatus? SubscriptionStatus { get; set; }
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public bool IsAnnualSubscription { get; set; }
        
        // Trainer Information
        public int TrainerProfileId { get; set; }
        public string TrainerName { get; set; } = null!;
        public string? TrainerHandle { get; set; }
        public bool IsTrainerVerified { get; set; }
        public decimal? TrainerRating { get; set; }
        public int? TrainerTotalClients { get; set; }
        
        // Payment Amount Details
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public decimal PlatformFee { get; set; }
        public decimal TrainerPayout { get; set; }
        
        // Payment Status & Method
        public PaymentStatus Status { get; set; }
        public PaymentMethod Method { get; set; }
        
        // Transaction Information
        public string? TransactionId { get; set; }
        public string? FailureReason { get; set; }
        
        // Payment Gateway Data
        public string? PaymentUrl { get; set; }
        public string? PaymobOrderId { get; set; }
        public string? PayPalOrderId { get; set; }
        
        // Dates
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? FailedAt { get; set; }
    }
}