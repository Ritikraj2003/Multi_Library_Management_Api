namespace Multi_Library_Management_Api.Models
{
    public class AttendanceLog
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int LibraryId { get; set; }  // stored directly for reliable filtering
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public bool AccessGranted { get; set; }

        // Navigation Properties
        public Student Student { get; set; } = null!;
    }
}
