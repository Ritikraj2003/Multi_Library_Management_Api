using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;

namespace Multi_Library_Management_Api.Repository
{
    public class RfidAccessRepository : IRfidAccessRepository
    {
        private readonly AppDbContext _context;

        public RfidAccessRepository(AppDbContext context) => _context = context;

        public async Task<Response<RfidTapResponseDto>> TapAsync(RfidTapRequestDto dto)
        {
            var response = new Response<RfidTapResponseDto>();

            if (string.IsNullOrWhiteSpace(dto.RFIDCode))
            {
                response.Success = false;
                response.Message = "RFID code is required.";
                return response;
            }

            var rfidCode = dto.RFIDCode.Trim();
            var tapTime = DateTime.UtcNow;

            try
            {
                var student = await _context.Students
                    .AsNoTracking()
                    .Include(s => s.StudentRegistrations.Where(r => r.Status == RegistrationStatus.Active))
                    .ThenInclude(r => r.Batch)
                    .FirstOrDefaultAsync(s =>
                        s.RFIDCode == rfidCode &&
                        s.LibraryId == dto.LibraryId);

                if (student == null)
                {
                    response.Success = false;
                    response.Message = "RFID card not registered for this library.";
                    return response;
                }

                var registration = student.StudentRegistrations
                    .OrderByDescending(r => r.RegistrationDate)
                    .FirstOrDefault();

                var (isAllowed, reason) = ValidateAccess(student, registration, tapTime);
                var inBatchWindow = IsInBatchWindow(registration, tapTime);

                var gateAllowed = isAllowed && inBatchWindow;
                var gateReason = reason;
                if (isAllowed && !inBatchWindow)
                {
                    gateAllowed = false;
                    gateReason = "Outside batch time window.";
                }

                var gateLog = new GateAccessLog
                {
                    StudentId = student.Id,
                    RFIDCode = rfidCode,
                    AccessDate = tapTime,
                    IsAllowed = gateAllowed,
                    Reason = gateReason
                };
                _context.GateAccessLogs.Add(gateLog);

                AttendanceLog? attendanceLog = null;
                var action = "GateOnly";

                if (isAllowed && inBatchWindow)
                {
                    var openSession = await _context.AttendanceLogs
                        .Where(a => a.StudentId == student.Id && a.ExitTime == null)
                        .OrderByDescending(a => a.EntryTime)
                        .FirstOrDefaultAsync();

                    if (openSession == null)
                    {
                        attendanceLog = new AttendanceLog
                        {
                            StudentId = student.Id,
                            EntryTime = tapTime,
                            AccessGranted = true
                        };
                        _context.AttendanceLogs.Add(attendanceLog);
                        action = "CheckIn";
                    }
                    else
                    {
                        openSession.ExitTime = tapTime;
                        attendanceLog = openSession;
                        action = "CheckOut";
                    }
                }

                await _context.SaveChangesAsync();

                response.Data = new RfidTapResponseDto
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    IsAllowed = gateAllowed,
                    Reason = gateReason,
                    Action = action,
                    TapTime = tapTime,
                    AttendanceLogId = attendanceLog?.Id,
                    GateAccessLogId = gateLog.Id
                };
                response.Success = true;
                response.Message = action switch
                {
                    "CheckIn" => "Check-in recorded.",
                    "CheckOut" => "Check-out recorded.",
                    _ when isAllowed && !inBatchWindow =>
                        "Gate log saved. Attendance not updated — outside batch time.",
                    _ => "Gate log saved. Access denied — attendance not updated."
                };
            }
            catch (Exception ex) when (IsDatabaseConnectionError(ex))
            {
                response.Success = false;
                response.Message =
                    "Could not reach the database. Check your internet, Hostinger MySQL remote access, and try again.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        private static (bool IsAllowed, string? Reason) ValidateAccess(
            Student student,
            StudentRegistration? registration,
            DateTime tapTime)
        {
            if (!student.IsActive)
                return (false, "Student account is inactive.");

            if (registration == null)
                return (false, "No active registration.");

            if (registration.DueDate.Date < tapTime.Date)
                return (false, "Registration fee is overdue.");

            return (true, null);
        }

        private static bool IsInBatchWindow(StudentRegistration? registration, DateTime tapTime)
        {
            if (registration?.Batch == null || !registration.Batch.IsActive)
                return true;

            return IsWithinBatchWindow(
                registration.Batch.StartTime,
                registration.Batch.EndTime,
                tapTime);
        }

        private static bool IsWithinBatchWindow(string startTime, string endTime, DateTime tapTime)
        {
            if (!TimeSpan.TryParse(startTime, out var start) ||
                !TimeSpan.TryParse(endTime, out var end))
                return true;

            var current = tapTime.TimeOfDay;
            if (start <= end)
                return current >= start && current <= end;

            return current >= start || current <= end;
        }

        private static bool IsDatabaseConnectionError(Exception ex)
        {
            for (var current = ex; current != null; current = current.InnerException)
            {
                if (current is SocketException or IOException)
                    return true;
            }

            return false;
        }
    }
}
