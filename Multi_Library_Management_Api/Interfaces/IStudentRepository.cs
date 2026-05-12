using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Interfaces
{
    public interface IStudentRepository
    {
        Task<Response<StudentResponseDto>> CreateAsync(CreateStudentDto dto);
        Task<Response<StudentResponseDto>> UpdateAsync(UpdateStudentDto dto);
        Task<Response<bool>> DeleteAsync(int id);
        Task<Response<StudentResponseDto>> GetByIdAsync(int id);
        Task<Response<PagedResult<StudentListDto>>> GetAllAsync(SearchQuery query);
    }
}
