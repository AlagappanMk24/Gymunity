using Gymunity.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gymunity.Application.DTOs.User.Payment
{
    public class InitiatePaymentRequest
    {
        [Required]
        public int SubscriptionId { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentMethod PaymentMethod { get; set; }

        public string? ReturnUrl { get; set; }
    }
}