using Gymunity.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.User.Subscriptions
{
    public class SubscribePackageRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Package ID")]
        public int PackageId { get; set; }

        public bool IsAnnual { get; set; } = false;

        public string? PromoCode { get; set; }

        // ✅ NEW: Payment method support
        public PaymentMethod? PaymentMethod { get; set; }

        // ✅ NEW: Return URL for payment confirmation
        public string? ReturnUrl { get; set; }
    }
}
