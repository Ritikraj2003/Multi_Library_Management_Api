using Microsoft.AspNetCore.Mvc;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;

namespace Multi_Library_Management_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AttendanceLocationController : ControllerBase
    {
        private readonly IAttendanceLocationRepository _repo;
        public AttendanceLocationController(IAttendanceLocationRepository repo) => _repo = repo;

        [HttpGet("{libraryId}")]
        public async Task<IActionResult> GetByLibraryId(int libraryId)
        {
            return Ok(await _repo.GetByLibraryIdAsync(libraryId));
        }

        [HttpPost]
        public async Task<IActionResult> Upsert([FromBody] UpsertLocationDto dto)
        {
            var location = new AttendanceLocation
            {
                LibraryId = dto.LibraryId,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                RadiusInMeters = dto.RadiusInMeters
            };
            return Ok(await _repo.UpsertAsync(location));
        }
    }

    public class UpsertLocationDto
    {
        public int LibraryId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double RadiusInMeters { get; set; }
    }
}
