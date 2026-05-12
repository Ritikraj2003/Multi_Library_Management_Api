namespace Multi_Library_Management_Api.Models
{
    public class RolePermission
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        // Navigation Properties
        public Role Role { get; set; } = null!;
        public Permission Permission { get; set; } = null!;
    }
}
