using Microsoft.AspNetCore.Mvc;
using Multi_Library_Management_Api.Interfaces;
using System.Threading.Tasks;

namespace Multi_Library_Management_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _repo;
        public DashboardController(IDashboardRepository repo) => _repo = repo;

        [HttpGet("stats/{libraryId}")]
        public async Task<IActionResult> GetStats(int libraryId)
        {
            var result = await _repo.GetDashboardStatsAsync(libraryId);
            return Ok(result);
        }
    }
}
