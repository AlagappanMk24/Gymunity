namespace Gymunity.Application.DTOs.User.Webhook
{
    public class PayPalWebhookPayload
    {
        public string Id { get; set; } = null!;  // Webhook event ID
        public string Event_Type { get; set; } = null!;  // "PAYMENT.CAPTURE.COMPLETED", "", 
        public string Create_Time { get; set; } = null!;
        public string Resource_Type { get; set; } = null!;
        public PayPalResource Resource { get; set; } = null!;
    }

    public class PayPalResource
    {
        public string Id { get; set; } = null!;  // Capture ID
        public string Status { get; set; } = null!;  // "COMPLETED"
        public PayPalAmount Amount { get; set; } = null!;
        public PayPalPurchaseUnit[] Purchase_Units { get; set; } = Array.Empty<PayPalPurchaseUnit>();

        // ✅ Additional properties
        public string? Create_Time { get; set; }
        public string? Update_Time { get; set; }
        public bool? Final_Capture { get; set; }
    }

    public class PayPalAmount
    {
        public string Currency_Code { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    public class PayPalPurchaseUnit
    {
        public string Reference_Id { get; set; } = null!;  // Your Payment ID
        public PayPalAmount? Amount { get; set; }
        public PayPalPayee? Payee { get; set; }
    }

    public class PayPalPayee
    {
        public string? Email_Address { get; set; }
        public string? Merchant_Id { get; set; }
    }
}