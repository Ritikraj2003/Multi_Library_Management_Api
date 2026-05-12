using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;

namespace Multi_Library_Management_Api.Interfaces
{
    public interface IAuthRepository
    {
        Task<Response<LoginResponseDto>> LoginAsync(LoginRequestDto request);
    }
}
