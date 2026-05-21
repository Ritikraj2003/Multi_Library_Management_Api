using System;
using System.ComponentModel.DataAnnotations;

namespace Multi_Library_Management_Api.Models
{
    public class AttendanceLocation
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int LibraryId { get; set; }
        
        [Required]
        public double Latitude { get; set; }
        
        [Required]
        public double Longitude { get; set; }
        
        [Required]
        public double RadiusInMeters { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Navigation
        public Library? Library { get; set; }
    }
}
