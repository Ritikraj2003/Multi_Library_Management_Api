using System.ComponentModel.DataAnnotations;

namespace Multi_Library_Management_Api.Models
{
    public class GeneralSetting
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int LibraryId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;
        
        [Required]
        public string Value { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }

        // Navigation
        public Library Library { get; set; } = null!;
    }
}
