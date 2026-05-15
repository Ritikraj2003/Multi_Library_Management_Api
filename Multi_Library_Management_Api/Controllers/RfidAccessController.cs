using Microsoft.AspNetCore.Mvc;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models.DTOs;

namespace Multi_Library_Management_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RfidAccessController : ControllerBase
    {
        private readonly IRfidAccessRepository _repo;

        public RfidAccessController(IRfidAccessRepository repo) => _repo = repo;

        [HttpPost]
        public async Task<IActionResult> Tap([FromBody] RfidTapRequestDto dto)
        {
            var result = await _repo.TapAsync(dto);
            return Ok(result);
        }
    }
}
