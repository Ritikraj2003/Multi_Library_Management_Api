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

        private DateTime GetIndianTime()
        {
            TimeZoneInfo indianTimeZone;
            try
            {
                // Try Windows ID
                indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback to Linux/Unix ID
                indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            }
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);
        }

        public async Task<Response<AttendanceLogDto>> MarkAttendanceAsync(MarkAttendanceDto dto)
        {
            // Find student by both StudentId and LibraryId to ensure they belong to this library
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == dto.StudentId && s.LibraryId == dto.LibraryId);
            if (student == null)
            {
                return new Response<AttendanceLogDto> { Success = false, Message = "Student not found in this library." };
            }

            // Directly fetch the active registration for this student in this library
            // (same student can have multiple registrations — cancelled/expired ones are ignored)
            var registration = await _context.StudentRegistrations
                .FirstOrDefaultAsync(r => r.StudentId == dto.StudentId
                                       && r.LibraryId == dto.LibraryId
                                       && r.Status == RegistrationStatus.Active);

            if (registration == null)
            {
                // Check why — give a meaningful message
                var anyReg = await _context.StudentRegistrations
                    .Where(r => r.StudentId == dto.StudentId && r.LibraryId == dto.LibraryId)
                    .OrderByDescending(r => r.Id)
                    .FirstOrDefaultAsync();

                if (anyReg == null)
                    return new Response<AttendanceLogDto> { Success = false, Message = "No registration found for this student." };

                if (anyReg.Status == RegistrationStatus.Cancelled)
                    return new Response<AttendanceLogDto> { Success = false, Message = "Your registration has been cancelled. Please contact the library." };

                if (anyReg.Status == RegistrationStatus.Expired)
                    return new Response<AttendanceLogDto> { Success = false, Message = "Your registration has expired. Please renew to mark attendance." };

                return new Response<AttendanceLogDto> { Success = false, Message = "No active registration found for this student." };
            }

            // Fetch attendance geofence location for this library from DB
            var location = await _context.AttendanceLocations
                .FirstOrDefaultAsync(al => al.LibraryId == dto.LibraryId);

            if (location == null)
            {
                return new Response<AttendanceLogDto> { Success = false, Message = "Attendance location is not configured for this library. Please contact your administrator." };
            }

            var distance = CalculateDistance(dto.Latitude, dto.Longitude, location.Latitude, location.Longitude);

            if (distance > location.RadiusInMeters)
            {
                return new Response<AttendanceLogDto> { Success = false, Message = $"You are too far from the library. Please be within {location.RadiusInMeters} meters. Distance is {Math.Round(distance, 2)} meters." };
            }

            var indianNow = GetIndianTime();
            var today = indianNow.Date;
            var log = await _context.AttendanceLogs
                .FirstOrDefaultAsync(a => a.StudentId == dto.StudentId && a.EntryTime >= today);

            if (log == null)
            {
                log = new AttendanceLog
                {
                    StudentId = dto.StudentId,
                    LibraryId = dto.LibraryId,
                    EntryTime = indianNow,
                    AccessGranted = true
                };
                _context.AttendanceLogs.Add(log);
            }
            else
            {
                log.ExitTime = indianNow;
                _context.AttendanceLogs.Update(log);
            }

            await _context.SaveChangesAsync();

            var logDto = new AttendanceLogDto
            {
                Id = log.Id,
                StudentId = log.StudentId,
                StudentName = student.FullName,
                FatherName = student.FatherName,
                Mobile = student.Mobile,
                Email = student.Email,
                Address = student.Address,
                Photo = student.Photo,
                DOB = student.DOB,
                EntryTime = log.EntryTime,
                ExitTime = log.ExitTime,
                AccessGranted = log.AccessGranted
            };

            return new Response<AttendanceLogDto> { Success = true, Message = "Attendance marked successfully.", Data = logDto };
        }

        public async Task<Response<List<AttendanceLogDto>>> GetTodayAttendanceAsync(int libraryId)
        {
            var startOfDay = GetIndianTime().Date;
            var logs = await _context.AttendanceLogs
                .Include(a => a.Student)
                .Where(a => a.LibraryId == libraryId && a.EntryTime >= startOfDay)
                .OrderByDescending(a => a.EntryTime)
                .Select(a => new AttendanceLogDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    StudentName = a.Student.FullName,
                    FatherName = a.Student.FatherName,
                    Mobile = a.Student.Mobile,
                    Email = a.Student.Email,
                    Address = a.Student.Address,
                    Photo = a.Student.Photo,
                    DOB = a.Student.DOB,
                    EntryTime = a.EntryTime,
                    ExitTime = a.ExitTime,
                    AccessGranted = a.AccessGranted
                }).ToListAsync();

            return new Response<List<AttendanceLogDto>> { Success = true, Data = logs };
        }

        public async Task<Response<List<AttendanceBatchStatDto>>> GetAttendanceByBatchAsync(int libraryId, string? date)
        {
            DateTime targetDate;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsed))
                targetDate = parsed.Date;
            else
                targetDate = GetIndianTime().Date;

            var nextDay = targetDate.AddDays(1);

            var stats = await _context.AttendanceLogs
                .Where(al => al.LibraryId == libraryId
                          && al.EntryTime >= targetDate
                          && al.EntryTime < nextDay)
                .Join(
                    _context.StudentRegistrations
                        .Where(r => r.LibraryId == libraryId && r.Status == RegistrationStatus.Active)
                        .Include(r => r.Batch),
                    al => al.StudentId,
                    sr => sr.StudentId,
                    (al, sr) => new { sr.Batch.Name }
                )
                .GroupBy(x => x.Name)
                .Select(g => new AttendanceBatchStatDto
                {
                    BatchName = g.Key,
                    AttendanceCount = g.Count()
                })
                .ToListAsync();

            return new Response<List<AttendanceBatchStatDto>> { Success = true, Data = stats };
        }
    }
}
