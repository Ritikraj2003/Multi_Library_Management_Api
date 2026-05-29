using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Interfaces
{
    public interface IStudentRegistrationRepository
    {
        Task<Response<StudentRegistrationResponseDto>> CreateAsync(CreateStudentRegistrationDto dto);
        Task<Response<StudentRegistrationResponseDto>> UpdateAsync(UpdateStudentRegistrationDto dto);
        Task<Response<bool>> DeleteAsync(int id);
        Task<Response<StudentRegistrationResponseDto>> GetByIdAsync(int id);
        Task<Response<PagedResult<StudentRegistrationListDto>>> GetAllAsync(SearchQuery query);
        Task<Response<bool>> RenewAsync(RenewRegistrationDto dto);
        Task<Response<List<PaymentResponseDto>>> GetPaymentHistoryAsync(int registrationId);
        Task<Response<SeatBatchStatusDto>> GetSeatAvailabilityAsync(int seatId, int libraryId, int? registrationId = null);
        Task<Response<PagedResult<StudentRegistrationListDto>>> GetDueStudentsAsync(SearchQuery query);
        Task<Response<PagedResult<StudentRegistrationListDto>>> GetTodayDueStudentsAsync(SearchQuery query);
        Task<Response<PagedResult<StudentRegistrationListDto>>> GetExpiredStudentsAsync(SearchQuery query);
        Task<Response<PagedResult<StudentRegistrationListDto>>> GetCancelledStudentsAsync(SearchQuery query);
        Task<Response<bool>> SendReceiptEmailAsync(SendReceiptEmailDto dto);
        Task<Response<StudentRegistrationResponseDto>> GetPublicReceiptAsync(int registrationId, int libraryId, int paymentId);
        Task<Response<List<int>>> GetActiveStudentIdsByLibraryAsync(int libraryId);
        Task<Response<bool>> HasActiveRegistrationAsync(int studentId);
    }
}
