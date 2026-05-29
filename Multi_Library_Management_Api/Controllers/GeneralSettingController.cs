using Microsoft.AspNetCore.Mvc;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models.DTOs;

namespace Multi_Library_Management_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class GeneralSettingController : ControllerBase
    {
        private readonly IGeneralSettingRepository _repo;
        public GeneralSettingController(IGeneralSettingRepository repo) => _repo = repo;

        [HttpGet("{libraryId}")]
        public async Task<IActionResult> GetByLibraryId(int libraryId)
        {
            return Ok(await _repo.GetByLibraryIdAsync(libraryId));
        }

        [HttpGet("{libraryId}")]
        public async Task<IActionResult> IsRazorpayVerified(int libraryId)
        {
            return Ok(await _repo.IsRazorpayVerifiedAsync(libraryId));
        }

        [HttpPost]
        public async Task<IActionResult> UpsertEmail([FromBody] UpsertEmailSettingsDto dto)
        {
            return Ok(await _repo.UpsertEmailSettingsAsync(dto));
        }

        [HttpPost]
        public async Task<IActionResult> UpsertRazorpay([FromBody] UpsertRazorpaySettingsDto dto)
        {
            return Ok(await _repo.UpsertRazorpaySettingsAsync(dto));
        }
    }
}
