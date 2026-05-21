using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;

namespace Multi_Library_Management_Api.Repository
{
    public class AttendanceLocationRepository : IAttendanceLocationRepository
    {
        private readonly AppDbContext _context;
        public AttendanceLocationRepository(AppDbContext context) => _context = context;

        public async Task<Response<AttendanceLocation>> GetByLibraryIdAsync(int libraryId)
        {
            var response = new Response<AttendanceLocation>();
            try
            {
                var location = await _context.AttendanceLocations
                    .FirstOrDefaultAsync(al => al.LibraryId == libraryId);
                
                if (location != null)
                {
                    response.Data = location;
                    response.Success = true;
                    response.Message = "Attendance location retrieved successfully.";
                }
                else
                {
                    response.Data = null;
                    response.Success = true;
                    response.Message = "No attendance location set for this library.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<Response<AttendanceLocation>> UpsertAsync(AttendanceLocation location)
        {
            var response = new Response<AttendanceLocation>();
            try
            {
                var existing = await _context.AttendanceLocations
                    .FirstOrDefaultAsync(al => al.LibraryId == location.LibraryId);

                if (existing != null)
                {
                    existing.Latitude = location.Latitude;
                    existing.Longitude = location.Longitude;
                    existing.RadiusInMeters = location.RadiusInMeters;
                    existing.UpdatedDate = DateTime.UtcNow;
                    _context.AttendanceLocations.Update(existing);
                    await _context.SaveChangesAsync();
                    response.Data = existing;
                    response.Message = "Attendance location updated successfully.";
                }
                else
                {
                    location.CreatedDate = DateTime.UtcNow;
                    _context.AttendanceLocations.Add(location);
                    await _context.SaveChangesAsync();
                    response.Data = location;
                    response.Message = "Attendance location created successfully.";
                }
                response.Success = true;
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
