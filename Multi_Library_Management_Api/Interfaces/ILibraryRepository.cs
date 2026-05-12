using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Interfaces
{
    public interface ILibraryRepository
    {
        Task<Response<CreateLibraryResponseDto>> CreateAsync(CreateLibraryDto dto);
        Task<Response<LibraryResponseDto>> UpdateAsync(UpdateLibraryDto dto);
        Task<Response<bool>> DeleteAsync(int id);
        Task<Response<LibraryResponseDto>> GetByIdAsync(int id);
        Task<Response<PagedResult<LibraryListDto>>> GetAllAsync(SearchQuery query);
    }
}
