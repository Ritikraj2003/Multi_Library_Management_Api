using Microsoft.AspNetCore.Mvc;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Controllers
{
    [ApiController]
    [Route("api/student-registration")]
    public class StudentRegistrationController : ControllerBase
    {
        private readonly IStudentRegistrationRepository _repo;
        public StudentRegistrationController(IStudentRegistrationRepository repo) => _repo = repo;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStudentRegistrationDto dto)
        {
            var result = await _repo.CreateAsync(dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SearchQuery query)
        {
            var result = await _repo.GetAllAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _repo.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentRegistrationDto dto)
        {
            dto.Id = id;
            var result = await _repo.UpdateAsync(dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.DeleteAsync(id);
            return Ok(result);
        }

        [HttpGet("due")]
        public async Task<IActionResult> GetDue([FromQuery] SearchQuery query)
        {
            var result = await _repo.GetDueStudentsAsync(query);
            return Ok(result);
        }

        [HttpGet("today-due")]
        public async Task<IActionResult> GetTodayDue([FromQuery] SearchQuery query)
        {
            var result = await _repo.GetTodayDueStudentsAsync(query);
            return Ok(result);
        }

        [HttpGet("expired")]
        public async Task<IActionResult> GetExpired([FromQuery] SearchQuery query)
        {
            var result = await _repo.GetExpiredStudentsAsync(query);
            return Ok(result);
        }

        [HttpGet("cancelled")]
        public async Task<IActionResult> GetCancelled([FromQuery] SearchQuery query)
        {
            var result = await _repo.GetCancelledStudentsAsync(query);
            return Ok(result);
        }

        [HttpPost("renew")]
        public async Task<IActionResult> Renew([FromBody] RenewRegistrationDto dto)
        {
            var result = await _repo.RenewAsync(dto);
            return Ok(result);
        }

        [HttpGet("payment-history/{registrationId}")]
        public async Task<IActionResult> GetPaymentHistory(int registrationId)
        {
            var result = await _repo.GetPaymentHistoryAsync(registrationId);
            return Ok(result);
        }

        [HttpGet("seat-availability/{seatId}/{libraryId}")]
        public async Task<IActionResult> GetSeatAvailability(int seatId, int libraryId, [FromQuery] int? registrationId)
        {
            var result = await _repo.GetSeatAvailabilityAsync(seatId, libraryId, registrationId);
            return Ok(result);
        }
    }
}
