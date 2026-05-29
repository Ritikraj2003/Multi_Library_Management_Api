namespace Multi_Library_Management_Api.Models.DTOs
{
    public class CreateOrderRequest
    {
        public int LibraryId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? Receipt { get; set; }
    }

    public class VerifyPaymentRequest
    {
        public int LibraryId { get; set; }
        public string RazorpayPaymentId { get; set; } = string.Empty;
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpaySignature { get; set; } = string.Empty;
    }
}
