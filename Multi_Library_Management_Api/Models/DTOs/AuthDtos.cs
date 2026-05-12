namespace Multi_Library_Management_Api.Models.DTOs
{
    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int? LibraryId { get; set; }
        public string? LibraryName { get; set; }
        public bool IsSuperadmin { get; set; }
        public List<string> Permissions { get; set; } = new();
    }
}
