namespace Multi_Library_Management_Api.Models
{
    public class GateAccessLog
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string RFIDCode { get; set; } = string.Empty;
        public DateTime AccessDate { get; set; }
        public bool IsAllowed { get; set; }
        public string? Reason { get; set; }

        // Navigation Properties
        public Student Student { get; set; } = null!;
    }
}
