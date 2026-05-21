namespace Multi_Library_Management_Api.Models.DTOs
{
    // ─── Permission DTOs ──────────────────────────────────────────────────────

    public class CreatePermissionDto
    {
        public string Name { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdatePermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class PermissionResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class PermissionListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    // ─── LibraryPermission DTOs ───────────────────────────────────────────────

    public class AssignLibraryPermissionsDto
    {
        public int LibraryId { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }

    public class LibraryPermissionResponseDto
    {
        public int Id { get; set; }
        public int LibraryId { get; set; }
        public string LibraryName { get; set; } = string.Empty;
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
    }

    // ─── RolePermission DTOs ──────────────────────────────────────────────────

    public class AssignPermissionsDto
    {
        public int RoleId { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }

    public class RolePermissionResponseDto
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
    }
}
