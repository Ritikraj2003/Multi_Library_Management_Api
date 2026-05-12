using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Interfaces
{
    public interface IUserRepository
    {
        Task<Response<UserResponseDto>> CreateAsync(CreateUserDto dto);
        Task<Response<UserResponseDto>> UpdateAsync(UpdateUserDto dto);
        Task<Response<bool>> DeleteAsync(int id);
        Task<Response<UserResponseDto>> GetByIdAsync(int id);
        Task<Response<PagedResult<UserListDto>>> GetAllAsync(SearchQuery query);
    }
}
