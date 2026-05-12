using Microsoft.AspNetCore.Mvc;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models.DTOs;

namespace Multi_Library_Management_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BatchController : ControllerBase
    {
        private readonly IBatchRepository _repo;
        public BatchController(IBatchRepository repo) => _repo = repo;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBatchDto dto)
        {
            return Ok(await _repo.CreateAsync(dto));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateBatchDto dto)
        {
            return Ok(await _repo.UpdateAsync(dto));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await _repo.DeleteAsync(id));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _repo.GetByIdAsync(id));
        }

        [HttpGet("{libraryId}")]
        public async Task<IActionResult> GetByLibraryId(int libraryId)
        {
            return Ok(await _repo.GetByLibraryIdAsync(libraryId));
        }
    }
}
