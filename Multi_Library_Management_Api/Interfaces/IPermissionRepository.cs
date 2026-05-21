using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Interfaces
{
    public interface IPermissionRepository
    {
        Task<Response<PermissionResponseDto>> CreateAsync(CreatePermissionDto dto);
        Task<Response<PermissionResponseDto>> UpdateAsync(UpdatePermissionDto dto);
        Task<Response<bool>> DeleteAsync(int id);
        Task<Response<PermissionResponseDto>> GetByIdAsync(int id);
        Task<Response<PagedResult<PermissionListDto>>> GetAllAsync(SearchQuery query);
        Task<Response<bool>> AssignPermissionsToRoleAsync(AssignPermissionsDto dto);
        Task<Response<List<RolePermissionResponseDto>>> GetPermissionsByRoleIdAsync(int roleId);

        // Library Permissions
        Task<Response<bool>> AssignPermissionsToLibraryAsync(AssignLibraryPermissionsDto dto);
        Task<Response<List<LibraryPermissionResponseDto>>> GetPermissionsByLibraryIdAsync(int libraryId);
    }
}
