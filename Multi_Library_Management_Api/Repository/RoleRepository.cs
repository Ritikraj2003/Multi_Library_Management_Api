using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context) => _context = context;

        public async Task<Response<RoleResponseDto>> CreateAsync(CreateRoleDto dto)
        {
            var response = new Response<RoleResponseDto>();
            try
            {
                var role = new Role
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    LibraryId = dto.LibraryId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
                response.Data = await BuildResponseAsync(role.Id);
                response.Success = true;
                response.Message = "Role created successfully.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<RoleResponseDto>> UpdateAsync(UpdateRoleDto dto)
        {
            var response = new Response<RoleResponseDto>();
            try
            {
                var role = await _context.Roles.FindAsync(dto.Id);
                if (role == null) { response.Success = false; response.Message = "Role not found."; return response; }
                role.Name = dto.Name;
                role.Description = dto.Description;
                role.IsActive = dto.IsActive;
                await _context.SaveChangesAsync();
                response.Data = await BuildResponseAsync(role.Id);
                response.Success = true;
                response.Message = "Role updated successfully.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<bool>> DeleteAsync(int id)
        {
            var response = new Response<bool>();
            try
            {
                var role = await _context.Roles.FindAsync(id);
                if (role == null) { response.Success = false; response.Message = "Role not found."; return response; }
                role.IsActive = false;
                await _context.SaveChangesAsync();
                response.Data = true; response.Success = true; response.Message = "Role deactivated.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<RoleResponseDto>> GetByIdAsync(int id)
        {
            var response = new Response<RoleResponseDto>();
            try
            {
                var dto = await BuildResponseAsync(id);
                if (dto == null) { response.Success = false; response.Message = "Role not found."; return response; }
                response.Data = dto; response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }
        public async Task<Response<PagedResult<RoleListDto>>> GetAllAsync(SearchQuery query)
        {
            var response = new Response<PagedResult<RoleListDto>>();
            try
            {
                var q = _context.Roles.Include(r => r.Library).AsQueryable();

                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                    q = q.Where(r => r.Name.Contains(query.SearchTerm));
                if (query.IsActive.HasValue)
                    q = q.Where(r => r.IsActive == query.IsActive.Value);
                if (query.LibraryId.HasValue)
                    q = q.Where(r => r.LibraryId == query.LibraryId.Value);

                var totalCount = await q.CountAsync();
                var items = await CommonQuery.ApplyPagination(q, query.PageNumber, query.PageSize)
                    .Select(r => new RoleListDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        LibraryId = r.LibraryId,
                        LibraryName = r.Library != null ? r.Library.Name : null,
                        IsActive = r.IsActive
                    }).ToListAsync();

                response.Data = CommonQuery.BuildPagedResult(items, totalCount, query.PageNumber, query.PageSize);
                response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }


        public async Task<Response<List<RoleListDto>>> GetByLibraryIdAsync(int libraryId)
        {
            var response = new Response<List<RoleListDto>>();
            try
            {
                var roles = await _context.Roles
                    .Where(r => r.LibraryId == libraryId && r.IsActive)
                    .Select(r => new RoleListDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        LibraryId = r.LibraryId,
                        LibraryName = r.Library != null ? r.Library.Name : null,
                        IsActive = r.IsActive
                    }).ToListAsync();

                response.Data = roles;
                response.Success = true;
                response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        private async Task<RoleResponseDto?> BuildResponseAsync(int id)
        {
            return await _context.Roles
                .Include(r => r.Library)
                .Where(r => r.Id == id)
                .Select(r => new RoleResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    LibraryId = r.LibraryId,
                    LibraryName = r.Library != null ? r.Library.Name : null,
                    IsActive = r.IsActive,
                    CreatedDate = r.CreatedDate
                }).FirstOrDefaultAsync();
        }
    }
}
