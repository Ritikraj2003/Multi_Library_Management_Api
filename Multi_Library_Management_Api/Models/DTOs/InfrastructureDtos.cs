namespace Multi_Library_Management_Api.Models.DTOs
{
    // ─── Floor DTOs ───────────────────────────────────────────────────────────

    public class CreateFloorDto
    {
        public int LibraryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int FloorNumber { get; set; }
    }

    public class UpdateFloorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int FloorNumber { get; set; }
        public bool IsActive { get; set; }
    }

    public class FloorResponseDto
    {
        public int Id { get; set; }
        public int LibraryId { get; set; }
        public string LibraryName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int FloorNumber { get; set; }
        public bool IsActive { get; set; }
    }

    public class FloorListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int FloorNumber { get; set; }
        public string LibraryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    // ─── TableSeat DTOs (merged Table + Seat) ────────────────────────────────

    public class CreateTableSeatDto
    {
        public int LibraryId { get; set; }
        public int FloorId { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
    }

    public class UpdateTableSeatDto
    {
        public int Id { get; set; }
        public int LibraryId { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class TableSeatResponseDto
    {
        public int Id { get; set; }
        public int LibraryId { get; set; }
        public string LibraryName { get; set; } = string.Empty;
        public int FloorId { get; set; }
        public string FloorName { get; set; } = string.Empty;
        public string TableNumber { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public bool IsOccupied { get; set; }
        public bool IsActive { get; set; }
    }

    public class TableSeatListDto
    {
        public int Id { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public string FloorName { get; set; } = string.Empty;
        public string LibraryName { get; set; } = string.Empty;
        public bool IsOccupied { get; set; }
        public bool IsActive { get; set; }
    }
}
