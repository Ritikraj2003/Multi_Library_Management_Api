using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Response<PaymentResponseDto>> CreateAsync(CreatePaymentDto dto);
        Task<Response<PaymentResponseDto>> GetByIdAsync(int id);
        Task<Response<PagedResult<PaymentListDto>>> GetAllAsync(SearchQuery query);
    }
}
