namespace Multi_Library_Management_Api.Models
{
    public class Batch
    {
        public int Id { get; set; }
        public int LibraryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation
        public Library Library { get; set; } = null!;
    }
}
