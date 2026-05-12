namespace Multi_Library_Management_Api.Models
{
    public class Floor
    {
        public int Id { get; set; }
        public int LibraryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int FloorNumber { get; set; }
        public bool IsActive { get; set; }

        // Navigation Properties
        public Library Library { get; set; } = null!;
        public ICollection<TableSeat> TableSeats { get; set; } = new List<TableSeat>();
    }
}
