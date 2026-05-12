namespace Multi_Library_Management_Api.Models.DTOs
{
    // ─── Role DTOs ────────────────────────────────────────────────────────────

    public class CreateRoleDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? LibraryId { get; set; }
    }

    public class UpdateRoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class RoleResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? LibraryId { get; set; }
        public string? LibraryName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class RoleListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? LibraryId { get; set; }
        public string? LibraryName { get; set; }
        public bool IsActive { get; set; }
    }
}
