using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;
        public PaymentRepository(AppDbContext context) => _context = context;

        public async Task<Response<PaymentResponseDto>> CreateAsync(CreatePaymentDto dto)
        {
            var response = new Response<PaymentResponseDto>();
            try
            {
                var registration = await _context.StudentRegistrations.FindAsync(dto.RegistrationId);
                if (registration == null) { response.Success = false; response.Message = "Registration not found."; return response; }

                var payment = new Payment
                {
                    RegistrationId = dto.RegistrationId,
                    Amount = dto.Amount,
                    PaymentDate = dto.PaymentDate,
                    NextDueDate = dto.NextDueDate,
                    PaymentMode = dto.PaymentMode,
                    TransactionId = dto.TransactionId,
                    Notes = dto.Notes,
                    CreatedBy = dto.CreatedBy
                };
                _context.Payments.Add(payment);

                // Update the registration's DueDate to the new NextDueDate
                registration.DueDate = dto.NextDueDate;
                registration.Status = RegistrationStatus.Active;
                await _context.SaveChangesAsync();

                response.Data = await BuildResponseAsync(payment.Id);
                response.Success = true; response.Message = "Payment recorded and due date updated.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<PaymentResponseDto>> GetByIdAsync(int id)
        {
            var response = new Response<PaymentResponseDto>();
            try
            {
                var dto = await BuildResponseAsync(id);
                if (dto == null) { response.Success = false; response.Message = "Payment not found."; return response; }
                response.Data = dto; response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<PagedResult<PaymentListDto>>> GetAllAsync(SearchQuery query)
        {
            var response = new Response<PagedResult<PaymentListDto>>();
            try
            {
                var q = _context.Payments
                    .Include(p => p.StudentRegistration).ThenInclude(r => r.Student)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                    q = q.Where(p => p.StudentRegistration.Student.FullName.Contains(query.SearchTerm));
                if (query.LibraryId.HasValue)
                    q = q.Where(p => p.StudentRegistration.Student.LibraryId == query.LibraryId.Value);

                var totalCount = await q.CountAsync();
                var items = await CommonQuery.ApplyPagination(q, query.PageNumber, query.PageSize)
                    .Select(p => new PaymentListDto
                    {
                        Id = p.Id,
                        StudentName = p.StudentRegistration.Student.FullName,
                        Amount = p.Amount,
                        PaymentDate = p.PaymentDate,
                        PaymentMode = p.PaymentMode
                    }).ToListAsync();

                response.Data = CommonQuery.BuildPagedResult(items, totalCount, query.PageNumber, query.PageSize);
                response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        private async Task<PaymentResponseDto?> BuildResponseAsync(int id) =>
            await _context.Payments
                .Include(p => p.StudentRegistration).ThenInclude(r => r.Student)
                .Include(p => p.CreatedByUser)
                .Where(p => p.Id == id)
                .Select(p => new PaymentResponseDto
                {
                    Id = p.Id, RegistrationId = p.RegistrationId,
                    StudentName = p.StudentRegistration.Student.FullName,
                    Amount = p.Amount, PaymentDate = p.PaymentDate, NextDueDate = p.NextDueDate,
                    PaymentMode = p.PaymentMode, TransactionId = p.TransactionId,
                    Notes = p.Notes, CreatedBy = p.CreatedBy, CreatedByName = p.CreatedByUser.FullName
                }).FirstOrDefaultAsync();
    }
}
