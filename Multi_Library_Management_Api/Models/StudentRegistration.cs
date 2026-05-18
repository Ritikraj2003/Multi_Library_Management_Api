namespace Multi_Library_Management_Api.Models
{
    public enum RegistrationStatus
    {
        Active = 1,
        Expired = 2,
        Cancelled = 3
    }

    public class StudentRegistration
    {
        public int Id { get; set; }
        public int LibraryId { get; set; }
        public int StudentId { get; set; }
        public int TableSeatId { get; set; }
        public int BatchId { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal MonthlyAmount { get; set; }
        public decimal SecurityAmount { get; set; }
        public string? Notes { get; set; }
        public string? RFIDCode { get; set; }
        public RegistrationStatus Status { get; set; }
        public int CreatedBy { get; set; }

        // Navigation Properties
        public Library Library { get; set; } = null!;
        public Student Student { get; set; } = null!;
        public TableSeat TableSeat { get; set; } = null!;
        public Batch Batch { get; set; } = null!;
        public User CreatedByUser { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
