using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using System.Threading.Tasks;

namespace Multi_Library_Management_Api.Interfaces
{
    public interface IDashboardRepository
    {
        Task<Response<DashboardStatsDto>> GetDashboardStatsAsync(int libraryId);
    }
}
