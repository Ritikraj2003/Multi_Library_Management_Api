using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Repository
{
    public class LibraryRepository : ILibraryRepository
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public LibraryRepository(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private async Task<string?> SaveFileAsync(IFormFile? file, string subFolder)
        {
            if (file == null || file.Length == 0) return null;

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads", subFolder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine("uploads", subFolder, fileName).Replace("\\", "/");
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE — Library + Auto Role + Auto User
        // ─────────────────────────────────────────────────────────────────────
        public async Task<Response<CreateLibraryResponseDto>> CreateAsync(CreateLibraryDto dto)
        {
            var response = new Response<CreateLibraryResponseDto>();

            try
            {
                // 1. Save Files
                var iconPath = await SaveFileAsync(dto.LibraryIconFile, "library");
                var docPath = await SaveFileAsync(dto.DocumentImageFile, "library");

                // 2. Create Library
                var library = new Library
                {
                    Name = dto.Name,
                    Address = dto.Address,
                    City = dto.City,
                    State = dto.State,
                    Pincode = dto.Pincode,
                    Mobile = dto.Mobile,
                    Email = dto.Email,
                    LibraryIcon = iconPath,
                    DocumentImage = docPath,
                    DocumentType = dto.DocumentType,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Libraries.Add(library);
                await _context.SaveChangesAsync();

                // 2. Load all active permissions
                var allPermissions = await _context.Permissions
                    .Where(p => p.IsActive)
                    .ToListAsync();

                // 3. Create Library Admin Role
                var roleName = $"{dto.Name} Admin";
                var role = new Role
                {
                    Name = roleName,
                    Description = $"Admin role for {dto.Name}",
                    LibraryId = library.Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();

                // 4. Assign all permissions to the role
                var rolePermissions = allPermissions.Select(p => new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = p.Id
                }).ToList();
                _context.RolePermissions.AddRange(rolePermissions);

                // 5. Build default admin credentials
                var slug = dto.Name.ToLower().Replace(" ", "");
                var adminEmail = $"admin@{slug}.com";
                var adminPassword = "Admin@123";

                // 6. Create Library Admin User
                var adminUser = new User
                {
                    FullName = $"{dto.Name} Admin",
                    Mobile = dto.Mobile ?? "0000000000",
                    Email = adminEmail,
                    Password = adminPassword,
                    RoleId = role.Id,
                    LibraryId = library.Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();

                response.Data = new CreateLibraryResponseDto
                {
                    Library = MapToResponseDto(library),
                    AdminEmail = adminEmail,
                    AdminPassword = adminPassword,
                    RoleName = roleName
                };
                response.Success = true;
                response.Message = "Library created successfully with admin role and user.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        // ─────────────────────────────────────────────────────────────────────
        // UPDATE
        // ─────────────────────────────────────────────────────────────────────
        public async Task<Response<LibraryResponseDto>> UpdateAsync(UpdateLibraryDto dto)
        {
            var response = new Response<LibraryResponseDto>();

            try
            {
                var library = await _context.Libraries.FindAsync(dto.Id);
                if (library == null)
                {
                    response.Success = false;
                    response.Message = "Library not found.";
                    return response;
                }

                if (dto.LibraryIconFile != null)
                {
                    library.LibraryIcon = await SaveFileAsync(dto.LibraryIconFile, "library");
                }
                if (dto.DocumentImageFile != null)
                {
                    library.DocumentImage = await SaveFileAsync(dto.DocumentImageFile, "library");
                }

                library.Name = dto.Name;
                library.Address = dto.Address;
                library.City = dto.City;
                library.State = dto.State;
                library.Pincode = dto.Pincode;
                library.Mobile = dto.Mobile;
                library.Email = dto.Email;
                library.DocumentType = dto.DocumentType;
                library.IsActive = dto.IsActive;

                await _context.SaveChangesAsync();

                response.Data = MapToResponseDto(library);
                response.Success = true;
                response.Message = "Library updated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE (soft delete)
        // ─────────────────────────────────────────────────────────────────────
        public async Task<Response<bool>> DeleteAsync(int id)
        {
            var response = new Response<bool>();

            try
            {
                var library = await _context.Libraries.FindAsync(id);
                if (library == null)
                {
                    response.Success = false;
                    response.Message = "Library not found.";
                    return response;
                }

                library.IsActive = false;
                await _context.SaveChangesAsync();

                response.Data = true;
                response.Success = true;
                response.Message = "Library deactivated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET BY ID
        // ─────────────────────────────────────────────────────────────────────
        public async Task<Response<LibraryResponseDto>> GetByIdAsync(int id)
        {
            var response = new Response<LibraryResponseDto>();

            try
            {
                var library = await _context.Libraries.FindAsync(id);
                if (library == null)
                {
                    response.Success = false;
                    response.Message = "Library not found.";
                    return response;
                }

                response.Data = MapToResponseDto(library);
                response.Success = true;
                response.Message = "Success";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET ALL (paginated + search)
        // ─────────────────────────────────────────────────────────────────────
        public async Task<Response<PagedResult<LibraryListDto>>> GetAllAsync(SearchQuery query)
        {
            var response = new Response<PagedResult<LibraryListDto>>();

            try
            {
                var q = _context.Libraries.AsQueryable();

                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                    q = q.Where(l => l.Name.Contains(query.SearchTerm) ||
                                     (l.City != null && l.City.Contains(query.SearchTerm)));

                if (query.IsActive.HasValue)
                    q = q.Where(l => l.IsActive == query.IsActive.Value);

                var totalCount = await q.CountAsync();

                var items = await CommonQuery.ApplyPagination(q, query.PageNumber, query.PageSize)
                    .Select(l => new LibraryListDto
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Address = l.Address,
                        City = l.City,
                        State = l.State,
                        Pincode = l.Pincode,
                        Mobile = l.Mobile,
                        Email = l.Email,
                        LibraryIcon = l.LibraryIcon,
                        DocumentImage = l.DocumentImage,
                        DocumentType = l.DocumentType,
                        IsActive = l.IsActive
                    }).ToListAsync();

                response.Data = CommonQuery.BuildPagedResult(items, totalCount, query.PageNumber, query.PageSize);
                response.Success = true;
                response.Message = "Success";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        private static LibraryResponseDto MapToResponseDto(Library l) => new()
        {
            Id = l.Id,
            Name = l.Name,
            Address = l.Address,
            City = l.City,
            State = l.State,
            Pincode = l.Pincode,
            Mobile = l.Mobile,
            Email = l.Email,
            LibraryIcon = l.LibraryIcon,
            DocumentImage = l.DocumentImage,
            DocumentType = l.DocumentType,
            IsActive = l.IsActive,
            CreatedDate = l.CreatedDate
        };
    }
}
