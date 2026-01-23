namespace Gymunity.Application.DTOs.User.Payment
{
    public class PaymentHistoryResponse
    {
        public int TotalPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public List<PaymentResponse> Payments { get; set; } = new();
    }
}