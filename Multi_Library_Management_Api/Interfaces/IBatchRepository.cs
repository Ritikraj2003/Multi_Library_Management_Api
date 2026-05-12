using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Interfaces
{
    public interface IBatchRepository
    {
        Task<Response<BatchResponseDto>> CreateAsync(CreateBatchDto dto);
        Task<Response<BatchResponseDto>> UpdateAsync(UpdateBatchDto dto);
        Task<Response<bool>> DeleteAsync(int id);
        Task<Response<BatchResponseDto>> GetByIdAsync(int id);
        Task<Response<List<BatchResponseDto>>> GetByLibraryIdAsync(int libraryId);
    }
}
