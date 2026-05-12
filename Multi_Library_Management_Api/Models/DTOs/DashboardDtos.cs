namespace Multi_Library_Management_Api.Models.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalStudents { get; set; }
        public int ActiveRegistrations { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalTables { get; set; }
        public List<PaymentModeStatDto> PaymentModes { get; set; } = new();
        public List<BatchStatDto> BatchStats { get; set; } = new();
    }

    public class PaymentModeStatDto
    {
        public string Mode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int Count { get; set; }
    }

    public class BatchStatDto
    {
        public string BatchName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
    }
}
