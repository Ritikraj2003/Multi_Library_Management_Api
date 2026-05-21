using System;
using System.ComponentModel.DataAnnotations;

namespace Multi_Library_Management_Api.Models.DTOs
{
    public class MarkAttendanceDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int LibraryId { get; set; }

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
        public string? FatherName { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Photo { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public bool AccessGranted { get; set; }
    }

    public class AttendanceBatchStatDto
    {
        public string BatchName { get; set; } = string.Empty;
        public int AttendanceCount { get; set; }
    }
}
