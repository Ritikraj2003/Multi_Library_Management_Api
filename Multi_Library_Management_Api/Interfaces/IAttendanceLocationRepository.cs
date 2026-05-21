using Multi_Library_Management_Api.Models;

namespace Multi_Library_Management_Api.Interfaces
{
    public interface IAttendanceLocationRepository
    {
        Task<Response<AttendanceLocation>> GetByLibraryIdAsync(int libraryId);
        Task<Response<AttendanceLocation>> UpsertAsync(AttendanceLocation location);
    }
}
