using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Interfaces
{
    public interface ITableSeatRepository
    {
        Task<Response<TableSeatResponseDto>> CreateAsync(CreateTableSeatDto dto);
        Task<Response<TableSeatResponseDto>> UpdateAsync(UpdateTableSeatDto dto);
        Task<Response<bool>> DeleteAsync(int id);
        Task<Response<TableSeatResponseDto>> GetByIdAsync(int id);
        Task<Response<PagedResult<TableSeatListDto>>> GetAllAsync(SearchQuery query);
        Task<Response<bool>> BulkUpdatePositionsAsync(List<UpdateTableSeatPositionDto> dtos);
    }
}
