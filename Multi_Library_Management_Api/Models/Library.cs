namespace Multi_Library_Management_Api.Models
{
    public class Library
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? LibraryIcon { get; set; }
        public string? DocumentImage { get; set; }
        public string? DocumentType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation Properties
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public ICollection<Floor> Floors { get; set; } = new List<Floor>();
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
