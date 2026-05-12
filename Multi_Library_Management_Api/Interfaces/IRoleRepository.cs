using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Interfaces
{
    public interface IRoleRepository
    {
        Task<Response<RoleResponseDto>> CreateAsync(CreateRoleDto dto);
        Task<Response<RoleResponseDto>> UpdateAsync(UpdateRoleDto dto);
        Task<Response<bool>> DeleteAsync(int id);
        Task<Response<RoleResponseDto>> GetByIdAsync(int id);
        Task<Response<PagedResult<RoleListDto>>> GetAllAsync(SearchQuery query);
        Task<Response<List<RoleListDto>>> GetByLibraryIdAsync(int libraryId);
    }
}
