using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Microsoft.Extensions.Configuration;

namespace Multi_Library_Management_Api.Repository
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AttendanceRepository(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371e3; // metres
            var p1 = lat1 * Math.PI / 180; // φ, λ in radians
            var p2 = lat2 * Math.PI / 180;
            var dp = (lat2 - lat1) * Math.PI / 180;
            var dl = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(dp / 2) * Math.Sin(dp / 2) +
                    Math.Cos(p1) * Math.Cos(p2) *
                    Math.Sin(dl / 2) * Math.Sin(dl / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // in metres
        }

        public async Task<Response<AttendanceLogDto>> MarkAttendanceAsync(MarkAttendanceDto dto)
        {
            var targetLat = _configuration.GetValue<double>("AttendanceLocation:Latitude");
            var targetLon = _configuration.GetValue<double>("AttendanceLocation:Longitude");
            var radius = _configuration.GetValue<double>("AttendanceLocation:RadiusInMeters");

            var distance = CalculateDistance(dto.Latitude, dto.Longitude, targetLat, targetLon);

            if (distance > radius)
            {
                return new Response<AttendanceLogDto> { Success = false, Message = $"You are too far from the library. Please be within {radius} meters. Distance is {Math.Round(distance, 2)} meters." };
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == dto.StudentId);
            if (student == null)
            {
                return new Response<AttendanceLogDto> { Success = false, Message = "Student not found." };
            }

            var today = DateTime.Now.Date;
            var log = await _context.AttendanceLogs
                .FirstOrDefaultAsync(a => a.StudentId == dto.StudentId && a.EntryTime >= today);

            if (log == null)
            {
                log = new AttendanceLog
                {
                    StudentId = dto.StudentId,
                    EntryTime = DateTime.Now,
                    AccessGranted = true
                };
                _context.AttendanceLogs.Add(log);
            }
            else
            {
                log.ExitTime = DateTime.Now;
                _context.AttendanceLogs.Update(log);
            }

            await _context.SaveChangesAsync();

            var logDto = new AttendanceLogDto
            {
                Id = log.Id,
                StudentId = log.StudentId,
                StudentName = student.FullName,
                Mobile = student.Mobile,
                EntryTime = log.EntryTime,
                ExitTime = log.ExitTime,
                AccessGranted = log.AccessGranted
            };

            return new Response<AttendanceLogDto> { Success = true, Message = "Attendance marked successfully.", Data = logDto };
        }

        public async Task<Response<List<AttendanceLogDto>>> GetTodayAttendanceAsync(int libraryId)
        {
            var startOfDay = DateTime.Now.Date;
            var logs = await _context.AttendanceLogs
                .Include(a => a.Student)
                .Where(a => a.Student.LibraryId == libraryId && a.EntryTime >= startOfDay)
                .OrderByDescending(a => a.EntryTime)
                .Select(a => new AttendanceLogDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    StudentName = a.Student.FullName,
                    Mobile = a.Student.Mobile,
                    EntryTime = a.EntryTime,
                    ExitTime = a.ExitTime,
                    AccessGranted = a.AccessGranted
                }).ToListAsync();

            return new Response<List<AttendanceLogDto>> { Success = true, Data = logs };
        }
    }
}
