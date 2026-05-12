using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Repository
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly AppDbContext _context;

        public PermissionRepository(AppDbContext context) => _context = context;

        public async Task<Response<PermissionResponseDto>> CreateAsync(CreatePermissionDto dto)
        {
            var response = new Response<PermissionResponseDto>();
            try
            {
                var permission = new Permission
                {
                    Name = dto.Name,
                    Module = dto.Module,
                    Description = dto.Description,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Permissions.Add(permission);
                await _context.SaveChangesAsync();
                response.Data = Map(permission); response.Success = true; response.Message = "Permission created.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<PermissionResponseDto>> UpdateAsync(UpdatePermissionDto dto)
        {
            var response = new Response<PermissionResponseDto>();
            try
            {
                var permission = await _context.Permissions.FindAsync(dto.Id);
                if (permission == null) { response.Success = false; response.Message = "Permission not found."; return response; }
                permission.Name = dto.Name;
                permission.Module = dto.Module;
                permission.Description = dto.Description;
                permission.IsActive = dto.IsActive;
                await _context.SaveChangesAsync();
                response.Data = Map(permission); response.Success = true; response.Message = "Permission updated.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<bool>> DeleteAsync(int id)
        {
            var response = new Response<bool>();
            try
            {
                var permission = await _context.Permissions.FindAsync(id);
                if (permission == null) { response.Success = false; response.Message = "Permission not found."; return response; }
                permission.IsActive = false;
                await _context.SaveChangesAsync();
                response.Data = true; response.Success = true; response.Message = "Permission deactivated.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<PermissionResponseDto>> GetByIdAsync(int id)
        {
            var response = new Response<PermissionResponseDto>();
            try
            {
                var permission = await _context.Permissions.FindAsync(id);
                if (permission == null) { response.Success = false; response.Message = "Permission not found."; return response; }
                response.Data = Map(permission); response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<PagedResult<PermissionListDto>>> GetAllAsync(SearchQuery query)
        {
            var response = new Response<PagedResult<PermissionListDto>>();
            try
            {
                var q = _context.Permissions.AsQueryable();

                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                    q = q.Where(p => p.Name.Contains(query.SearchTerm) || p.Module.Contains(query.SearchTerm));
                if (query.IsActive.HasValue)
                    q = q.Where(p => p.IsActive == query.IsActive.Value);

                var totalCount = await q.CountAsync();
                var items = await CommonQuery.ApplyPagination(q, query.PageNumber, query.PageSize)
                    .Select(p => new PermissionListDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Module = p.Module,
                        IsActive = p.IsActive
                    }).ToListAsync();

                response.Data = CommonQuery.BuildPagedResult(items, totalCount, query.PageNumber, query.PageSize);
                response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<bool>> AssignPermissionsToRoleAsync(AssignPermissionsDto dto)
        {
            var response = new Response<bool>();
            try
            {
                // Remove existing assignments for this role
                var existing = _context.RolePermissions.Where(rp => rp.RoleId == dto.RoleId);
                _context.RolePermissions.RemoveRange(existing);

                // Add new assignments
                var newAssignments = dto.PermissionIds.Select(pid => new RolePermission
                {
                    RoleId = dto.RoleId,
                    PermissionId = pid
                });
                _context.RolePermissions.AddRange(newAssignments);
                await _context.SaveChangesAsync();

                response.Data = true; response.Success = true; response.Message = "Permissions assigned to role.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<List<RolePermissionResponseDto>>> GetPermissionsByRoleIdAsync(int roleId)
        {
            var response = new Response<List<RolePermissionResponseDto>>();
            try
            {
                var items = await (
                    from rp in _context.RolePermissions
                    join r in _context.Roles on rp.RoleId equals r.Id
                    join p in _context.Permissions on rp.PermissionId equals p.Id
                    where rp.RoleId == roleId
                    select new RolePermissionResponseDto
                    {
                        Id = rp.Id,
                        RoleId = rp.RoleId,
                        RoleName = r.Name,
                        PermissionId = rp.PermissionId,
                        PermissionName = p.Name,
                        Module = p.Module
                    }
                ).ToListAsync();

                response.Data = items; response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        private static PermissionResponseDto Map(Permission p) => new()
        {
            Id = p.Id, Name = p.Name, Module = p.Module,
            Description = p.Description, IsActive = p.IsActive, CreatedDate = p.CreatedDate
        };
    }
}
