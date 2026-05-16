using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;

namespace Multi_Library_Management_Api.Interfaces
{
    public interface IAuthRepository
    {
        Task<Response<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task<Response<string>> ForgotPasswordAsync(string email);
        Task<Response<string>> ResetPasswordAsync(ResetPasswordRequestDto request);
    }
}
