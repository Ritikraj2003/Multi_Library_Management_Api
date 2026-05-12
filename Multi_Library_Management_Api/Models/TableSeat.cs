namespace Multi_Library_Management_Api.Models
{
    public class TableSeat
    {
        public int Id { get; set; }
        public int LibraryId { get; set; }
        public int FloorId { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public bool IsOccupied { get; set; }
        public bool IsActive { get; set; }

        // Navigation Properties
        public Library Library { get; set; } = null!;
        public Floor Floor { get; set; } = null!;
        public ICollection<StudentRegistration> StudentRegistrations { get; set; } = new List<StudentRegistration>();
    }
}
