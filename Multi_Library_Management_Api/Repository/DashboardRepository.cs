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
                    .CountAsync(s => s.LibraryId == libraryId && s.IsActive);

                // 2. Active Registrations
                stats.ActiveRegistrations = await _context.StudentRegistrations
                    .CountAsync(r => r.LibraryId == libraryId && r.Status == RegistrationStatus.Active);

                // 3. Total Revenue
                stats.TotalRevenue = await _context.Payments
                    .Where(p => p.LibraryId == libraryId)
                    .SumAsync(p => p.Amount);

                // 5. Total Tables
                stats.TotalTables = await _context.TableSeats
                    .Where(ts => ts.LibraryId == libraryId && ts.IsActive)
                    .Select(ts => ts.TableNumber)
                    .Distinct()
                    .CountAsync();

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
    }
}
