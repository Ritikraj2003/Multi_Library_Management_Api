using System.ComponentModel.DataAnnotations;

namespace Multi_Library_Management_Api.Models
{
    public class GeneralSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LibraryId { get; set; }

        [StringLength(256)]
        public string? Email { get; set; }

        [StringLength(256)]
        public string? EmailSmtp { get; set; }

        public int? EmailPort { get; set; }

        [StringLength(500)]
        public string? EmailAppPassword { get; set; }

        [StringLength(256)]
        public string? RazorpayKey { get; set; }

        [StringLength(500)]
        public string? RazorpaySecretKey { get; set; }

        public bool IsRazorpayVerified { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        public Library Library { get; set; } = null!;
    }
}
