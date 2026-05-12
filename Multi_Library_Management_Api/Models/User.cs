namespace Multi_Library_Management_Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsSuperadmin { get; set; }
        public int RoleId { get; set; }
        public int? LibraryId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation Properties
        public Role Role { get; set; } = null!;
        public Library? Library { get; set; }
    }
}
