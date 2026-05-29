namespace Multi_Library_Management_Api.Interfaces
{
    public interface IRazorpayService
    {
        Task<bool> ValidateRazorpayKeysAsync(string keyId, string keySecret);
    }
}
