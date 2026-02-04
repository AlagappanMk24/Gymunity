namespace Gymunity.Application.DTOs.User.Webhook
{
    public class PaymobWebhookPayload
    {
        public string Type { get; set; } = null!;  // "TRANSACTION"
        public PaymobTransaction Obj { get; set; } = null!;
    }

    public class PaymobTransaction
    {
        public int Id { get; set; }
        public bool Success { get; set; }
        public decimal Amount_Cents { get; set; }  // 49900 = 499 EGP
        public string Currency { get; set; } = null!;
        public string Created_At { get; set; } = null!;

        //  Additional useful properties
        public bool Is_Refunded { get; set; }
        public bool Is_Auth { get; set; }
        public bool Is_Capture { get; set; }
        public bool Is_Standalone_Payment { get; set; }
        public bool Is_Voided { get; set; }
        public bool Pending { get; set; }
        public string? Error_Occured { get; set; }

        // Order & Source data
        public PaymobOrder Order { get; set; } = null!;
        public PaymobSourceData? Source_Data { get; set; }
    }

    public class PaymobOrder
    {
        public int Id { get; set; }
        public string Merchant_Order_Id { get; set; } = null!;  // Your Payment ID
        public decimal Amount_Cents { get; set; }
    }

    public class PaymobSourceData
    {
        public string? Type { get; set; }  // "card", "wallet", etc.
        public string? Sub_Type { get; set; }  // "MasterCard", "Visa", etc.
        public string? Pan { get; set; }  // Masked card number
    }
}