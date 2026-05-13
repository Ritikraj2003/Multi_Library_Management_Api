using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public UserRepository(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private async Task<string> SaveFileAsync(IFormFile file, string subDirectory)
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", subDirectory);
            if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine("uploads", subDirectory, fileName).Replace("\\", "/");
        }

        public async Task<Response<UserResponseDto>> CreateAsync(CreateUserDto dto)
        {
            var response = new Response<UserResponseDto>();
            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                {
                    response.Success = false; response.Message = "Email already in use."; return response;
                }
                string? profileImage = null;
                if (dto.ProfileImageFile != null)
                {
                    profileImage = await SaveFileAsync(dto.ProfileImageFile, "users");
                }

                var user = new User
                {
                    FullName = dto.FullName, Mobile = dto.Mobile, Email = dto.Email,
                    Password = dto.Password, RoleId = dto.RoleId, LibraryId = dto.LibraryId,
                    IsSuperadmin = dto.IsSuperadmin,
                    ProfileImage = profileImage,
                    IsActive = true, CreatedDate = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                response.Data = await BuildResponseAsync(user.Id);
                response.Success = true; response.Message = "User created.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<UserResponseDto>> UpdateAsync(UpdateUserDto dto)
        {
            var response = new Response<UserResponseDto>();
            try
            {
                var user = await _context.Users.FindAsync(dto.Id);
                if (user == null) { response.Success = false; response.Message = "User not found."; return response; }

                if (dto.ProfileImageFile != null)
                {
                    user.ProfileImage = await SaveFileAsync(dto.ProfileImageFile, "users");
                }

                user.FullName = dto.FullName; user.Mobile = dto.Mobile; user.Email = dto.Email;
                user.RoleId = dto.RoleId; user.LibraryId = dto.LibraryId; 
                user.IsSuperadmin = dto.IsSuperadmin;
                user.IsActive = dto.IsActive;
                if (!string.IsNullOrEmpty(dto.Password))
                    user.Password = dto.Password;

                await _context.SaveChangesAsync();
                response.Data = await BuildResponseAsync(user.Id);
                response.Success = true; response.Message = "User updated.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<bool>> DeleteAsync(int id)
        {
            var response = new Response<bool>();
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) { response.Success = false; response.Message = "User not found."; return response; }
                user.IsActive = false;
                await _context.SaveChangesAsync();
                response.Data = true; response.Success = true; response.Message = "User deactivated.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<UserResponseDto>> GetByIdAsync(int id)
        {
            var response = new Response<UserResponseDto>();
            try
            {
                var dto = await BuildResponseAsync(id);
                if (dto == null) { response.Success = false; response.Message = "User not found."; return response; }
                response.Data = dto; response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<PagedResult<UserListDto>>> GetAllAsync(SearchQuery query)
        {
            var response = new Response<PagedResult<UserListDto>>();
            try
            {
                var q = _context.Users.Include(u => u.Role).Include(u => u.Library).AsQueryable();

                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                    q = q.Where(u => u.FullName.Contains(query.SearchTerm) || u.Email.Contains(query.SearchTerm));
                if (query.IsActive.HasValue)
                    q = q.Where(u => u.IsActive == query.IsActive.Value);
                if (query.LibraryId.HasValue)
                    q = q.Where(u => u.LibraryId == query.LibraryId.Value);

                var totalCount = await q.CountAsync();
                var items = await CommonQuery.ApplyPagination(q, query.PageNumber, query.PageSize)
                    .Select(u => new UserListDto
                    {
                        Id = u.Id, FullName = u.FullName, Email = u.Email, Mobile = u.Mobile,
                        RoleName = u.Role.Name,
                        LibraryName = u.Library != null ? u.Library.Name : null,
                        IsSuperadmin = u.IsSuperadmin,
                        ProfileImage = u.ProfileImage,
                        IsActive = u.IsActive
                    }).ToListAsync();

                response.Data = CommonQuery.BuildPagedResult(items, totalCount, query.PageNumber, query.PageSize);
                response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        private async Task<UserResponseDto?> BuildResponseAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Library)
                .Where(u => u.Id == id)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id, FullName = u.FullName, Mobile = u.Mobile, Email = u.Email,
                    RoleId = u.RoleId, RoleName = u.Role.Name,
                    LibraryId = u.LibraryId, LibraryName = u.Library != null ? u.Library.Name : null,
                    IsSuperadmin = u.IsSuperadmin,
                    ProfileImage = u.ProfileImage,
                    IsActive = u.IsActive, CreatedDate = u.CreatedDate
                }).FirstOrDefaultAsync();
        }
    }
}
