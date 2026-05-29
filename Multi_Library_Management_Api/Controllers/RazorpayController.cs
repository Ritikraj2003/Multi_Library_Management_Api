using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models.DTOs;
using Razorpay.Api;

namespace Multi_Library_Management_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RazorpayController : ControllerBase
    {
        private readonly IGeneralSettingRepository _settingsRepo;

        public RazorpayController(IGeneralSettingRepository settingsRepo)
        {
            _settingsRepo = settingsRepo;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest req)
        {
            var (keyId, keySecret, error) = await GetRazorpayKeysAsync(req.LibraryId);
            if (error != null)
                return BadRequest(new { success = false, message = error });

            if (req.Amount <= 0)
                return BadRequest(new { success = false, message = "Amount must be greater than zero." });

            var client = new RazorpayClient(keyId!, keySecret!);
            var options = new Dictionary<string, object>
            {
                { "amount", (long)(req.Amount * 100) },
                { "currency", req.Currency ?? "INR" },
                { "receipt", req.Receipt ?? $"rcpt_{Guid.NewGuid():N}" },
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
            var (_, keySecret, error) = await GetRazorpayKeysAsync(req.LibraryId);
            if (error != null)
                return BadRequest(new { success = false, message = error });

            var generatedSignature = CalculateSha256Hash(
                $"{req.RazorpayOrderId}|{req.RazorpayPaymentId}", keySecret!);

            if (generatedSignature == req.RazorpaySignature)
                return Ok(new { success = true, message = "Payment verified successfully." });

            return BadRequest(new { success = false, message = "Invalid signature." });
        }

        private async Task<(string? KeyId, string? KeySecret, string? Error)> GetRazorpayKeysAsync(int libraryId)
        {
            var verifiedResult = await _settingsRepo.IsRazorpayVerifiedAsync(libraryId);
            if (!verifiedResult.Success || !verifiedResult.Data)
                return (null, null, "Razorpay is not verified for this library.");

            var settingsResult = await _settingsRepo.GetByLibraryIdAsync(libraryId);
            if (!settingsResult.Success || settingsResult.Data == null)
                return (null, null, "Settings not found.");

            var keyId = settingsResult.Data.RazorpayKey;
            var keySecret = settingsResult.Data.RazorpaySecretKey;

            if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
                return (null, null, "Razorpay keys not configured.");

            return (keyId, keySecret, null);
        }

        private static string CalculateSha256Hash(string text, string secret)
        {
            var encoding = new UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(text);
            using var hmacsha256 = new HMACSHA256(keyByte);
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
        }
    }
}
