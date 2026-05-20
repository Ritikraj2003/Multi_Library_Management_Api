using System;
using System.ComponentModel.DataAnnotations;

namespace Multi_Library_Management_Api.Models.DTOs
{
    public class MarkAttendanceDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
    }

    public class AttendanceLogDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public bool AccessGranted { get; set; }
    }
}
