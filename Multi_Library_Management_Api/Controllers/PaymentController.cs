using Microsoft.AspNetCore.Mvc;
using Multi_Library_Management_Api.Interfaces;
using Razorpay.Api;
using System.Security.Cryptography;
using System.Text;

namespace Multi_Library_Management_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PaymentController : ControllerBase
    {
        private readonly IGeneralSettingRepository _settingsRepo;

        public PaymentController(IGeneralSettingRepository settingsRepo)
        {
            _settingsRepo = settingsRepo;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest req)
        {
            var settingsResult = await _settingsRepo.GetByLibraryIdAsync(req.LibraryId);
            if (!settingsResult.Success || settingsResult.Data == null) return BadRequest(new { success = false, message = "Settings not found." });

            var keyId = settingsResult.Data.FirstOrDefault(s => s.Key == "keyId")?.Value;
            var keySecret = settingsResult.Data.FirstOrDefault(s => s.Key == "keySecret")?.Value;

            if (string.IsNullOrEmpty(keyId) || string.IsNullOrEmpty(keySecret))
                return BadRequest(new { success = false, message = "Razorpay keys not configured." });

            var client = new RazorpayClient(keyId, keySecret);
            var options = new Dictionary<string, object>
            {
                { "amount", (long)(req.Amount * 100) }, // INR to paise
                { "currency", req.Currency ?? "INR" },
                { "receipt", req.Receipt ?? $"rcpt_{Guid.NewGuid().ToString("N")}" },
                { "payment_capture", 1 }
            };

                        try
            {
                Order order = client.Order.Create(options);
                return Ok(new
                {
                    success = true,
                    key = keyId,
                    orderId = order["id"].ToString(),
                    amount = order["amount"],
                    currency = order["currency"]
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Razorpay API Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentRequest req)
        {
            var settingsResult = await _settingsRepo.GetByLibraryIdAsync(req.LibraryId);
            if (!settingsResult.Success || settingsResult.Data == null) return BadRequest(new { success = false, message = "Settings not found." });

            var keySecret = settingsResult.Data.FirstOrDefault(s => s.Key == "keySecret")?.Value;
            if (string.IsNullOrEmpty(keySecret)) return BadRequest(new { success = false, message = "Razorpay secret not configured." });

            string generatedSignature = CalculateSha256Hash($"{req.RazorpayOrderId}|{req.RazorpayPaymentId}", keySecret);

            if (generatedSignature == req.RazorpaySignature)
            {
                return Ok(new { success = true, message = "Payment verified successfully." });
            }
            return BadRequest(new { success = false, message = "Invalid signature." });
        }

        private string CalculateSha256Hash(string text, string secret)
        {
            var encoding = new UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(text);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
            }
        }
    }

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
