using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;

namespace Multi_Library_Management_Api.Repository
{
    /// <summary>
    /// Repository class handling the business logic for RFID Card taps, gate access validation, and student attendance logs.
    /// </summary>
    public class RfidAccessRepository : IRfidAccessRepository
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="RfidAccessRepository"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public RfidAccessRepository(AppDbContext context) => _context = context;

        /// <summary>
        /// Processes an RFID tap event at the gate: validates the card, checks student and registration status, 
        /// creates gate access logs, and manages the student's entry/exit attendance sessions.
        /// </summary>
        /// <param name="dto">The DTO containing the RFID card code and the library identifier.</param>
        /// <returns>A response indicating whether access was granted, along with log details.</returns>
        public async Task<Response<RfidTapResponseDto>> TapAsync(RfidTapRequestDto dto)
        {
            var response = new Response<RfidTapResponseDto>();

            // ─── STEP 1: VALIDATE REQUEST DTO ─────────────────────────────────
            if (string.IsNullOrWhiteSpace(dto.RFIDCode))
            {
                response.Success = false;
                response.Message = "RFID code is required.";
                return response;
            }

            var rfidCode = dto.RFIDCode.Trim();

            // ─── STEP 2: TIMEZONE CONVERSION TO IST (INDIAN STANDARD TIME) ────
            // Since the system operates under Indian Standard Time (+05:30), we convert UTC time to IST
            // to ensure completely accurate date comparisons (e.g. due dates and batch schedules).
            TimeZoneInfo istZone;
            try
            {
                // Try retrieving by standard Windows ID
                istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback to IANA standard ID for Linux/Docker hosting
                istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            }
            var tapTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

            try
            {
                // ─── STEP 3: LOOKUP STUDENT REGISTRATION BY RFID AND LIBRARY ─────
                // Querying the student registration table directly where the RFID card code is registered.
                var registration = await _context.StudentRegistrations
                    .Include(r => r.Student)
                    .Include(r => r.Batch)
                    .FirstOrDefaultAsync(r =>
                        r.RFIDCode == rfidCode &&
                        r.LibraryId == dto.LibraryId);

                // Label A: If no registration whatsoever matches the RFID card
                if (registration == null)
                {
                    response.Success = false;
                    response.Message = "RFID card is not registered for this library.";
                    return response;
                }

                var student = registration.Student;

                // Label B: If registration exists but is NOT Active (e.g., Expired or Cancelled)
                if (registration.Status != RegistrationStatus.Active)
                {
                    // Log the denied gate access attempt
                    var gateDeniedLog = new GateAccessLog
                    {
                        StudentId = student.Id,
                        RFIDCode = rfidCode,
                        AccessDate = tapTime,
                        IsAllowed = false,
                        Reason = $"Access Denied: Student registration is {registration.Status}."
                    };
                    _context.GateAccessLogs.Add(gateDeniedLog);
                    await _context.SaveChangesAsync();

                    response.Success = false;
                    response.Message = $"Access Denied: Registration is {registration.Status}.";
                    return response;
                }

                // ─── STEP 4: ACCESS RULE VALIDATIONS ─────────────────────────────
                // Check if the student account is active and if the registration fee is overdue.
                var (isAllowed, reason) = ValidateAccess(student, registration, tapTime);

                // Check if the current time matches the student's registered batch time window.
                var inBatchWindow = IsInBatchWindow(registration, tapTime);

                // Determine final access decision
                var gateAllowed = isAllowed && inBatchWindow;
                var gateReason = reason;

                if (isAllowed && !inBatchWindow)
                {
                    gateAllowed = false;
                    gateReason = "Outside batch time window.";
                }

                // ─── STEP 5: SAVE GATE ACCESS ATTEMPT LOG ───────────────────────
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

                // ─── STEP 6: ATTENDANCE SESSION MANAGEMENT ───────────────────────
                // If access is permitted at the gate, check the student in or out.
                if (isAllowed && inBatchWindow)
                {
                    // Look for an open attendance session (entry recorded but exit is null)
                    var openSession = await _context.AttendanceLogs
                        .Where(a => a.StudentId == student.Id && a.ExitTime == null)
                        .OrderByDescending(a => a.EntryTime)
                        .FirstOrDefaultAsync();

                    if (openSession == null)
                    {
                        // Label C: Create a new Check-in session
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
                        // Label D: Close the active session (Check-out)
                        openSession.ExitTime = tapTime;
                        attendanceLog = openSession;
                        action = "CheckOut";
                    }
                }

                // Save all database alterations atomically
                await _context.SaveChangesAsync();

                // ─── STEP 7: BUILD COMPREHENSIVE RESPONSE ────────────────────────
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
                    "CheckIn" => "Check-in recorded successfully.",
                    "CheckOut" => "Check-out recorded successfully.",
                    _ when isAllowed && !inBatchWindow =>
                        "Gate log saved. Access denied: Outside batch time window.",
                    _ => $"Gate log saved. Access denied: {gateReason}"
                };
            }
            catch (Exception ex) when (IsDatabaseConnectionError(ex))
            {
                response.Success = false;
                response.Message =
                    "Could not reach the database. Please check host connections and remote access configurations.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"An error occurred during gate processing: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Validates general eligibility rules for student gate entry.
        /// </summary>
        private static (bool IsAllowed, string? Reason) ValidateAccess(
            Student student,
            StudentRegistration? registration,
            DateTime tapTime)
        {
            // Rule 1: Student profile must be active
            if (!student.IsActive)
                return (false, "Student account status is inactive.");

            // Rule 2: Active registration record must be present
            if (registration == null)
                return (false, "No active registration record found.");

            // Rule 3: Registration due date must not be exceeded
            if (registration.DueDate.Date < tapTime.Date)
                return (false, "Registration fee is overdue.");

            return (true, null);
        }

        /// <summary>
        /// Checks if the current tap time is within the student's assigned batch window.
        /// </summary>
        private static bool IsInBatchWindow(StudentRegistration? registration, DateTime tapTime)
        {
            // If no batch is set or batch is inactive, bypass window checks
            if (registration?.Batch == null || !registration.Batch.IsActive)
                return true;

            return IsWithinBatchWindow(
                registration.Batch.StartTime,
                registration.Batch.EndTime,
                tapTime);
        }

        /// <summary>
        /// Validates if the current time of day lies within the start and end boundary times.
        /// </summary>
        private static bool IsWithinBatchWindow(string startTime, string endTime, DateTime tapTime)
        {
            if (!TimeSpan.TryParse(startTime, out var start) ||
                !TimeSpan.TryParse(endTime, out var end))
                return true;

            var current = tapTime.TimeOfDay;
            if (start <= end)
                return current >= start && current <= end;

            // Handle batches that span overnight
            return current >= start || current <= end;
        }

        /// <summary>
        /// Identifies database network timeouts and communication exceptions.
        /// </summary>
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
