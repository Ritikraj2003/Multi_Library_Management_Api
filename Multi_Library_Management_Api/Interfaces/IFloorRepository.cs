using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Interfaces
{
    public interface IFloorRepository
    {
        Task<Response<FloorResponseDto>> CreateAsync(CreateFloorDto dto);
        Task<Response<FloorResponseDto>> UpdateAsync(UpdateFloorDto dto);
        Task<Response<bool>> DeleteAsync(int id);
        Task<Response<FloorResponseDto>> GetByIdAsync(int id);
        Task<Response<PagedResult<FloorListDto>>> GetAllAsync(SearchQuery query);
    }
}
