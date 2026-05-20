using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Models;

namespace Multi_Library_Management_Api.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<Response<AttendanceLogDto>> MarkAttendanceAsync(MarkAttendanceDto dto);
        Task<Response<List<AttendanceLogDto>>> GetTodayAttendanceAsync(int libraryId);
    }
}
