using Microsoft.AspNetCore.Mvc;
using Multi_Library_Management_Api.Interfaces;

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

        [HttpPost]
        public async Task<IActionResult> Upsert([FromBody] UpsertSettingDto dto)
        {
            return Ok(await _repo.UpsertAsync(dto.LibraryId, dto.Key, dto.Value));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await _repo.DeleteAsync(id));
        }
    }

    public class UpsertSettingDto
    {
        public int LibraryId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
