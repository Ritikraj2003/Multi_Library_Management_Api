using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models.DTOs;

namespace Multi_Library_Management_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceRepository _attendanceRepository;

        public AttendanceController(IAttendanceRepository attendanceRepository)
        {
            _attendanceRepository = attendanceRepository;
        }

        [HttpPost("Mark")]
        [AllowAnonymous]
        public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _attendanceRepository.MarkAttendanceAsync(dto);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("Today/{libraryId}")]
        [Authorize]
        public async Task<IActionResult> GetTodayAttendance(int libraryId)
        {
            var response = await _attendanceRepository.GetTodayAttendanceAsync(libraryId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("BatchStats/{libraryId}")]
        [Authorize]
        public async Task<IActionResult> GetAttendanceByBatch(int libraryId, [FromQuery] string? date)
        {
            var response = await _attendanceRepository.GetAttendanceByBatchAsync(libraryId, date);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }
    }
}
