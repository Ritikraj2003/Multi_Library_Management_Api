using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using System.Threading.Tasks;

namespace Multi_Library_Management_Api.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;
        public DashboardRepository(AppDbContext context) => _context = context;

        public async Task<Response<DashboardStatsDto>> GetDashboardStatsAsync(int libraryId)
        {
            var response = new Response<DashboardStatsDto>();
            try
            {
                var stats = new DashboardStatsDto();

                // 1. Total Students
                stats.TotalStudents = await _context.Students
                    .CountAsync(s => s.LibraryId == libraryId);

                // 1b. Expired Students (Assuming status might indicate or just inactive for now, or based on registration)
                stats.ExpiredStudents = await _context.StudentRegistrations
                    .CountAsync(r => r.LibraryId == libraryId && r.Status == RegistrationStatus.Expired);

                var today = DateTime.UtcNow.Date;

                // 1c. Today Renewals (Using registration start date as proxy for now)
                stats.TodayRenewals = await _context.StudentRegistrations
                    .CountAsync(r => r.LibraryId == libraryId && r.RegistrationDate.Date == today);

                // 1d. Pending Fees
                // Approximated by total unpaid from registrations? Or skipped if no field. We'll set it to 0 for now if no simple way to calculate.
                stats.PendingFees = 0; // Replace with actual logic if Pending Amount field exists.

                // 2. Active Registrations
                stats.ActiveRegistrations = await _context.StudentRegistrations
                    .CountAsync(r => r.LibraryId == libraryId && r.Status == RegistrationStatus.Active);

                // 3. Total Revenue
                stats.TotalRevenue = await _context.Payments
                    .Where(p => p.LibraryId == libraryId)
                    .SumAsync(p => p.Amount);

                // 3a. Today Collection
                stats.TodayCollection = await _context.Payments
                    .Where(p => p.LibraryId == libraryId && p.PaymentDate.Date == today)
                    .SumAsync(p => p.Amount);

                // 5. Total Seats
                stats.TotalSeats = await _context.TableSeats
                    .CountAsync(ts => ts.LibraryId == libraryId && ts.IsActive);

                stats.OccupiedSeats = await _context.TableSeats
                    .CountAsync(ts => ts.LibraryId == libraryId && ts.IsActive && ts.IsOccupied);

                stats.AvailableSeats = stats.TotalSeats - stats.OccupiedSeats;

                // 5. Total Tables
                stats.TotalTables = await _context.TableSeats
                    .Where(ts => ts.LibraryId == libraryId && ts.IsActive)
                    .Select(ts => ts.TableNumber)
                    .Distinct()
                    .CountAsync();

                // Total Batches
                stats.TotalBatches = await _context.Batches
                    .CountAsync(b => b.LibraryId == libraryId && b.IsActive);

                // 4. Payment Modes distribution
                stats.PaymentModes = await _context.Payments
                    .Where(p => p.LibraryId == libraryId)
                    .GroupBy(p => p.PaymentMode)
                    .Select(g => new PaymentModeStatDto
                    {
                        Mode = g.Key,
                        TotalAmount = g.Sum(p => p.Amount),
                        Count = g.Count()
                    }).ToListAsync();

                // 6. Batch distribution
                stats.BatchStats = await _context.StudentRegistrations
                    .Include(r => r.Batch)
                    .Where(r => r.LibraryId == libraryId && r.Status == RegistrationStatus.Active)
                    .GroupBy(r => r.Batch.Name)
                    .Select(g => new BatchStatDto
                    {
                        BatchName = g.Key,
                        StudentCount = g.Count()
                    }).ToListAsync();

                // 7. Gender distribution
                var genderData = await _context.StudentRegistrations
                    .Include(r => r.Student)
                    .Where(r => r.LibraryId == libraryId && r.Status == RegistrationStatus.Active)
                    .ToListAsync();
                    
                stats.GenderStats = genderData
                    .GroupBy(r => string.IsNullOrWhiteSpace(r.Student.Gender) ? "Unknown" : r.Student.Gender)
                    .Select(g => new GenderStatDto
                    {
                        Gender = g.Key,
                        Count = g.Count()
                    }).ToList();

                response.Data = stats;
                response.Success = true;
                response.Message = "Dashboard statistics retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<Response<RevenueAnalyticsDto>> GetRevenueAnalyticsAsync(int libraryId, string timeframe)
        {
            var response = new Response<RevenueAnalyticsDto>();
            try
            {
                var result = new RevenueAnalyticsDto();
                
                // Get Monthly Revenue Data for the last 6 months
                var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
                sixMonthsAgo = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

                var payments = await _context.Payments
                    .Where(p => p.LibraryId == libraryId && p.PaymentDate >= sixMonthsAgo)
                    .ToListAsync();

                for (int i = 0; i < 6; i++)
                {
                    var targetMonth = sixMonthsAgo.AddMonths(i);
                    var monthRevenue = payments
                        .Where(p => p.PaymentDate.Year == targetMonth.Year && p.PaymentDate.Month == targetMonth.Month)
                        .Sum(p => p.Amount);

                    result.MonthlyData.Add(new MonthlyRevenueDto
                    {
                        Month = targetMonth.ToString("MMM yyyy"),
                        Revenue = monthRevenue
                    });
                }

                // Payment Modes
                result.PaymentModes = await _context.Payments
                    .Where(p => p.LibraryId == libraryId)
                    .GroupBy(p => p.PaymentMode)
                    .Select(g => new PaymentModeStatDto
                    {
                        Mode = g.Key,
                        TotalAmount = g.Sum(x => x.Amount),
                        Count = g.Count()
                    }).ToListAsync();

                response.Data = result;
                response.Success = true;
                response.Message = "Revenue analytics retrieved.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<Response<DashboardAlertsDto>> GetDashboardAlertsAsync(int libraryId)
        {
            var response = new Response<DashboardAlertsDto>();
            try
            {
                var alerts = new DashboardAlertsDto();
                var today = DateTime.UtcNow.Date;

                // Expiring today (Due Date is today)
                alerts.ExpiringToday = await _context.StudentRegistrations
                    .Include(r => r.Student)
                    .Include(r => r.Batch)
                    .Where(r => r.LibraryId == libraryId && r.DueDate.Date == today && r.Status == RegistrationStatus.Active)
                    .Select(r => new AlertStudentDto
                    {
                        StudentId = r.StudentId,
                        Name = r.Student.FullName,
                        Phone = r.Student.Mobile,
                        Plan = r.Batch != null ? r.Batch.Name : "N/A"
                    }).ToListAsync();

                // Pending Dues (Placeholder, assuming we don't have a direct Dues table, using a simple query or keeping it empty for now)
                alerts.PendingDues = new List<AlertPendingDueDto>(); 

                response.Data = alerts;
                response.Success = true;
                response.Message = "Alerts retrieved.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}

