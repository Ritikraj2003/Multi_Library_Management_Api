namespace Multi_Library_Management_Api.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int LibraryId { get; set; }
        public int RegistrationId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime NextDueDate { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
        public int CreatedBy { get; set; }

        // Navigation Properties
        public Library Library { get; set; } = null!;
        public StudentRegistration StudentRegistration { get; set; } = null!;
        public User CreatedByUser { get; set; } = null!;
    }
}
