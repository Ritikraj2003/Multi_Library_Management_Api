namespace Multi_Library_Management_Api.Models
{
    public class LibraryPermission
    {
        public int Id { get; set; }
        public int LibraryId { get; set; }
        public int PermissionId { get; set; }

        public Library Library { get; set; } = null!;
        public Permission Permission { get; set; } = null!;
    }
}
