using Microsoft.AspNetCore.Mvc;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionRepository _repo;
        public PermissionController(IPermissionRepository repo) => _repo = repo;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePermissionDto dto)
        {
            var result = await _repo.CreateAsync(dto);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdatePermissionDto dto)
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

        [HttpPost]
        public async Task<IActionResult> AssignToRole([FromBody] AssignPermissionsDto dto)
        {
            var result = await _repo.AssignPermissionsToRoleAsync(dto);
            return Ok(result);
        }

        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetByRole(int roleId)
        {
            var result = await _repo.GetPermissionsByRoleIdAsync(roleId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AssignToLibrary([FromBody] AssignLibraryPermissionsDto dto)
        {
            var result = await _repo.AssignPermissionsToLibraryAsync(dto);
            return Ok(result);
        }

        [HttpGet("{libraryId}")]
        public async Task<IActionResult> GetByLibrary(int libraryId)
        {
            var result = await _repo.GetPermissionsByLibraryIdAsync(libraryId);
            return Ok(result);
        }
    }
}
