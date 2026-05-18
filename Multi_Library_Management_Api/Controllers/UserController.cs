using Microsoft.AspNetCore.Mvc;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _repo;
        public UserController(IUserRepository repo) => _repo = repo;

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Create([FromForm] CreateUserDto dto)
        {
            var result = await _repo.CreateAsync(dto);
            return Ok(result);
        }

        [HttpPut]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Update([FromForm] UpdateUserDto dto)
        {
            var result = await _repo.UpdateAsync(dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.DeleteAsync(id);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _repo.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SearchQuery query)
        {
            var result = await _repo.GetAllAsync(query);
            return Ok(result);
        }
    }
}
