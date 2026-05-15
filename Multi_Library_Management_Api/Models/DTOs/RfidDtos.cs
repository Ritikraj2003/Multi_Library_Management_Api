namespace Multi_Library_Management_Api.Models.DTOs
{
    public class RfidTapRequestDto
    {
        public string RFIDCode { get; set; } = string.Empty;
        public int LibraryId { get; set; }
    }

    public class RfidTapResponseDto
    {
        public int? StudentId { get; set; }
        public string? StudentName { get; set; }
        public bool IsAllowed { get; set; }
        public string? Reason { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime TapTime { get; set; }
        public int? AttendanceLogId { get; set; }
        public int? GateAccessLogId { get; set; }
    }
}
