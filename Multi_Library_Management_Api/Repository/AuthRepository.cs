using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Helpers;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;

namespace Multi_Library_Management_Api.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthRepository(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<Response<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            var response = new Response<LoginResponseDto>();
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Library)
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.Password == request.Password);

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "Invalid email or password.";
                    return response;
                }

                if (!user.IsActive)
                {
                    response.Success = false;
                    response.Message = "User account is inactive.";
                    return response;
                }

                // Get Permissions
                var permissions = await _context.RolePermissions
                    .Include(rp => rp.Permission)
                    .Where(rp => rp.RoleId == user.RoleId)
                    .Select(rp => rp.Permission.Name)
                    .ToListAsync();

                var loginResponse = new LoginResponseDto
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    RoleId = user.RoleId,
                    RoleName = user.Role.Name,
                    LibraryId = user.LibraryId,
                    LibraryName = user.Library?.Name,
                    LibraryIcon = user.Library?.LibraryIcon,
                    IsSuperadmin = user.IsSuperadmin,
                    Permissions = permissions
                };

                var key = _configuration["Jwt:Key"] ?? "YourSuperSecretKeyWithAtLeast32CharsLength!!";
                var issuer = _configuration["Jwt:Issuer"] ?? "MultiLibrarySystem";
                var audience = _configuration["Jwt:Audience"] ?? "MultiLibraryUser";

                loginResponse.Token = JwtHelper.GenerateToken(loginResponse, key, issuer, audience);

                response.Data = loginResponse;
                response.Success = true;
                response.Message = "Login successful.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
