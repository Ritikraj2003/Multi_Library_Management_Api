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
        private readonly IEmailService _emailService;

        public AuthRepository(AppDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
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
                var permissionsQuery = _context.RolePermissions
                    .Include(rp => rp.Permission)
                    .Where(rp => rp.RoleId == user.RoleId)
                    .AsQueryable();

                if (user.LibraryId.HasValue)
                {
                    var libraryPermissionIds = await _context.LibraryPermissions
                        .Where(lp => lp.LibraryId == user.LibraryId.Value)
                        .Select(lp => lp.PermissionId)
                        .ToListAsync();
                    
                    permissionsQuery = permissionsQuery.Where(rp => libraryPermissionIds.Contains(rp.PermissionId));
                }

                var permissions = await permissionsQuery
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
        public async Task<Response<string>> ForgotPasswordAsync(string email)
        {
            var response = new Response<string>();
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "Email address not found.";
                    return response;
                }

                var key = _configuration["Jwt:Key"] ?? "YourSuperSecretKeyWithAtLeast32CharsLength!!";
                var issuer = _configuration["Jwt:Issuer"] ?? "MultiLibrarySystem";
                var audience = _configuration["Jwt:Audience"] ?? "MultiLibraryUser";

                // Generate JWT reset token (5 minute expiry)
                var token = JwtHelper.GenerateResetToken(user.Email, key, issuer, audience);
                
                var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                var resetLink = $"http://localhost:4200/auth/reset-password?token={token}&t={timestamp}";
                
                var body = $"<h3>Password Reset Request</h3><p>Please click the link below to reset your password. This link will expire in 5 minutes.</p><p><a href='{resetLink}'>Reset Password</a></p>";
                
                await _emailService.SendSystemEmailAsync(user.Email, "Password Reset Request", body);

                response.Data = token; // Sending token to frontend as requested
                response.Success = true;
                response.Message = "Password reset link sent to your email.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<Response<string>> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var response = new Response<string>();
            try
            {
                var key = _configuration["Jwt:Key"] ?? "YourSuperSecretKeyWithAtLeast32CharsLength!!";
                var issuer = _configuration["Jwt:Issuer"] ?? "MultiLibrarySystem";
                var audience = _configuration["Jwt:Audience"] ?? "MultiLibraryUser";

                // Validate JWT token
                var principal = JwtHelper.ValidateToken(request.Token, key, issuer, audience);
                if (principal == null || principal.FindFirst("Purpose")?.Value != "ResetPassword")
                {
                    response.Success = false;
                    response.Message = "Invalid or expired reset link.";
                    return response;
                }

                var email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found.";
                    return response;
                }

                user.Password = request.NewPassword; 
                user.IsActive = true; // Activate account on password reset
                await _context.SaveChangesAsync();

                response.Success = true;
                response.Message = "Password has been reset successfully.";
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
