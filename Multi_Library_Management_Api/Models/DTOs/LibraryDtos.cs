namespace Multi_Library_Management_Api.Models.DTOs
{
    // ─── Library DTOs ─────────────────────────────────────────────────────────

    public class CreateLibraryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
    }

    public class UpdateLibraryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
    }

    public class LibraryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateLibraryResponseDto
    {
        public LibraryResponseDto Library { get; set; } = null!;
        public string AdminEmail { get; set; } = string.Empty;
        public string AdminPassword { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }

    public class LibraryListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
    }
}
