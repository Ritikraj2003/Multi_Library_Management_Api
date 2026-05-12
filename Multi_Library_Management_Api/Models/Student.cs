namespace Multi_Library_Management_Api.Models
{
    public class Student
    {
        public int Id { get; set; }
        public int LibraryId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? FatherName { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? RFIDCode { get; set; }
        public string? Photo { get; set; } // This will be StudentImage
        public string? DocumentImage { get; set; }
        public string? DocumentType { get; set; }
        public DateTime? DOB { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation Properties
        public Library Library { get; set; } = null!;
        public ICollection<StudentRegistration> StudentRegistrations { get; set; } = new List<StudentRegistration>();
        public ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();
        public ICollection<GateAccessLog> GateAccessLogs { get; set; } = new List<GateAccessLog>();
    }
}
