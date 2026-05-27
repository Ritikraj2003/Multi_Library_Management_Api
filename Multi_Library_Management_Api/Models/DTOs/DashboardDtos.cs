namespace Multi_Library_Management_Api.Models.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int ExpiredStudents { get; set; }
        public int TodayRenewals { get; set; }
        public decimal PendingFees { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayCollection { get; set; }
        public int TotalSeats { get; set; }
        public int OccupiedSeats { get; set; }
        public int AvailableSeats { get; set; }
        public decimal RevenueGrowthPercent { get; set; }

        // Keep existing lists for backwards compatibility or basic charts
        public int ActiveRegistrations { get; set; }
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

    public class RevenueAnalyticsDto
    {
        public List<MonthlyRevenueDto> MonthlyData { get; set; } = new();
        public List<PaymentModeStatDto> PaymentModes { get; set; } = new();
    }

    public class MonthlyRevenueDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class DashboardAlertsDto
    {
        public List<AlertStudentDto> ExpiringToday { get; set; } = new();
        public List<AlertPendingDueDto> PendingDues { get; set; } = new();
    }

    public class AlertStudentDto
    {
        public int StudentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
    }

    public class AlertPendingDueDto
    {
        public int StudentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
