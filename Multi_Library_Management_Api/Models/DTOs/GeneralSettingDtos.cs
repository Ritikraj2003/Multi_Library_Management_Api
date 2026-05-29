namespace Multi_Library_Management_Api.Models.DTOs
{
    public class UpsertEmailSettingsDto
    {
        public int LibraryId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string EmailSmtp { get; set; } = string.Empty;
        public int EmailPort { get; set; }
        public string EmailAppPassword { get; set; } = string.Empty;
    }

    public class UpsertRazorpaySettingsDto
    {
        public int LibraryId { get; set; }
        public string RazorpayKey { get; set; } = string.Empty;
        public string RazorpaySecretKey { get; set; } = string.Empty;
    }
}
