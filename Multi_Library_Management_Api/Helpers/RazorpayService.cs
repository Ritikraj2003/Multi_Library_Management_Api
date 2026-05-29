using System.Net.Http.Headers;
using System.Text;
using Multi_Library_Management_Api.Interfaces;

namespace Multi_Library_Management_Api.Helpers
{
    public class RazorpayService : IRazorpayService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RazorpayService> _logger;

        public RazorpayService(IHttpClientFactory httpClientFactory, ILogger<RazorpayService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<bool> ValidateRazorpayKeysAsync(string keyId, string keySecret)
        {
            if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
                return false;

            try
            {
                var client = _httpClientFactory.CreateClient();
                var byteArray = Encoding.ASCII.GetBytes($"{keyId}:{keySecret}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(byteArray));

                var response = await client.GetAsync("https://api.razorpay.com/v1/payments?count=1");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Razorpay key validation failed for Key ID starting with {KeyPrefix}", keyId.Length > 8 ? keyId[..8] : keyId);
                return false;
            }
        }
    }
}
