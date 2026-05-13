namespace Multi_Library_Management_Api.Models.DTOs
{
    // ─── User DTOs ────────────────────────────────────────────────────────────

    public class CreateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public int? LibraryId { get; set; }
        public bool IsSuperadmin { get; set; }
        public IFormFile? ProfileImageFile { get; set; }
    }

    public class UpdateUserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public int RoleId { get; set; }
        public int? LibraryId { get; set; }
        public bool IsSuperadmin { get; set; }
        public IFormFile? ProfileImageFile { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int? LibraryId { get; set; }
        public string? LibraryName { get; set; }
        public bool IsSuperadmin { get; set; }
        public string? ProfileImage { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class UserListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? LibraryName { get; set; }
        public bool IsSuperadmin { get; set; }
        public string? ProfileImage { get; set; }
        public bool IsActive { get; set; }
    }
}
