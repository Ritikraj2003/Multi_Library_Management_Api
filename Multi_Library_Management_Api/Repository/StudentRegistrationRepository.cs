using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Repository
{
    public class StudentRegistrationRepository : IStudentRegistrationRepository
    {
        private readonly AppDbContext _context;
        public StudentRegistrationRepository(AppDbContext context) => _context = context;

        public async Task<Response<StudentRegistrationResponseDto>> CreateAsync(CreateStudentRegistrationDto dto)
        {
            var response = new Response<StudentRegistrationResponseDto>();
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Validate Student exists
                    var student = await _context.Students.FindAsync(dto.StudentId);
                    if (student == null) { response.Success = false; response.Message = "Student not found."; return response; }

                    // 2. Validate Seat available in this batch
                    var seat = await _context.TableSeats.FindAsync(dto.TableSeatId);
                    if (seat == null) { response.Success = false; response.Message = "Seat not found."; return response; }

                    var isBatchOccupied = await _context.StudentRegistrations
                        .AnyAsync(r => r.TableSeatId == dto.TableSeatId && r.BatchId == dto.BatchId && r.Status == RegistrationStatus.Active);
                    
                    if (isBatchOccupied) { response.Success = false; response.Message = "Seat is already occupied in this batch."; return response; }

                    // 3. Check active registration exists
                    var hasActive = await _context.StudentRegistrations.AnyAsync(r => r.StudentId == dto.StudentId && r.Status == RegistrationStatus.Active);
                    if (hasActive) { response.Success = false; response.Message = "Student already has an active registration."; return response; }

                    // 4. Create StudentRegistration
                    var registration = new StudentRegistration
                    {
                        LibraryId = dto.LibraryId,
                        StudentId = dto.StudentId, 
                        TableSeatId = dto.TableSeatId,
                        BatchId = dto.BatchId,
                        RegistrationDate = DateTime.UtcNow,
                        StartDate = dto.StartDate, DueDate = dto.DueDate,
                        MonthlyAmount = dto.MonthlyAmount, SecurityAmount = dto.SecurityAmount,
                        Notes = dto.Notes, Status = RegistrationStatus.Active,
                        CreatedBy = dto.CreatedBy
                    };
                    _context.StudentRegistrations.Add(registration);
                    await _context.SaveChangesAsync(); // Save to get Id for payment

                    // 5. Create first Payment history
                    var payment = new Payment
                    {
                        LibraryId = dto.LibraryId,
                        RegistrationId = registration.Id,
                        Amount = dto.MonthlyAmount,
                        PaymentDate = DateTime.UtcNow,
                        NextDueDate = dto.DueDate,
                        PaymentMode = "Cash",
                        CreatedBy = dto.CreatedBy
                    };
                    _context.Payments.Add(payment);

                    // 6. Global seat status removed as it is now batch-wise
                    // seat.IsOccupied = true;
                    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    response.Data = await BuildResponseAsync(registration.Id);
                    response.Success = true; response.Message = "Student registered successfully.";
                }
                catch (Exception ex) 
                { 
                    await transaction.RollbackAsync();
                    response.Success = false; response.Message = ex.Message; 
                }
                return response;
            });
        }

        public async Task<Response<StudentRegistrationResponseDto>> UpdateAsync(UpdateStudentRegistrationDto dto)
        {
            var response = new Response<StudentRegistrationResponseDto>();
            try
            {
                var registration = await _context.StudentRegistrations
                    .Include(r => r.TableSeat)
                    .FirstOrDefaultAsync(r => r.Id == dto.Id);
                if (registration == null) { response.Success = false; response.Message = "Registration not found."; return response; }

                registration.DueDate = dto.DueDate;
                registration.MonthlyAmount = dto.MonthlyAmount;
                registration.Notes = dto.Notes;

                if (dto.Status != 0 && dto.Status != registration.Status)
                {
                    registration.Status = dto.Status;
                    if (dto.Status == RegistrationStatus.Cancelled || dto.Status == RegistrationStatus.Expired)
                        registration.TableSeat.IsOccupied = false;
                    else if (dto.Status == RegistrationStatus.Active)
                        registration.TableSeat.IsOccupied = true;
                }

                await _context.SaveChangesAsync();
                response.Data = await BuildResponseAsync(registration.Id);
                response.Success = true; response.Message = "Registration updated.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<bool>> DeleteAsync(int id)
        {
            var response = new Response<bool>();
            try
            {
                var registration = await _context.StudentRegistrations
                    .Include(r => r.TableSeat)
                    .FirstOrDefaultAsync(r => r.Id == id);
                if (registration == null) { response.Success = false; response.Message = "Registration not found."; return response; }

                registration.Status = RegistrationStatus.Cancelled;
                if (registration.TableSeat != null)
                {
                    registration.TableSeat.IsOccupied = false;
                }
                await _context.SaveChangesAsync();
                response.Data = true; response.Success = true; response.Message = "Registration cancelled.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<StudentRegistrationResponseDto>> GetByIdAsync(int id)
        {
            var response = new Response<StudentRegistrationResponseDto>();
            try
            {
                var dto = await BuildResponseAsync(id);
                if (dto == null) { response.Success = false; response.Message = "Registration not found."; return response; }
                response.Data = dto; response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<PagedResult<StudentRegistrationListDto>>> GetAllAsync(SearchQuery query)
        {
            var response = new Response<PagedResult<StudentRegistrationListDto>>();
            try
            {
                var q = _context.StudentRegistrations
                    .Include(r => r.Student).Include(r => r.TableSeat).AsQueryable();

                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                    q = q.Where(r => r.Student.FullName.Contains(query.SearchTerm));
                if (query.LibraryId.HasValue)
                    q = q.Where(r => r.Student.LibraryId == query.LibraryId.Value);

                // Exclude Cancelled by default in 'All' view
                q = q.Where(r => r.Status != RegistrationStatus.Cancelled);

                var totalCount = await q.CountAsync();
                var items = await CommonQuery.ApplyPagination(q, query.PageNumber, query.PageSize)
                    .Select(r => new StudentRegistrationListDto
                    {
                        Id = r.Id, StudentId = r.StudentId, StudentName = r.Student.FullName,
                        Mobile = r.Student.Mobile,
                        TableSeatId = r.TableSeatId, SeatNumber = r.TableSeat.SeatNumber,
                        BatchId = r.BatchId, BatchName = r.Batch.Name,
                        StartDate = r.StartDate, DueDate = r.DueDate, 
                        MonthlyAmount = r.MonthlyAmount,
                        Status = r.Status.ToString()
                    }).ToListAsync();

                response.Data = CommonQuery.BuildPagedResult(items, totalCount, query.PageNumber, query.PageSize);
                response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<bool>> RenewAsync(RenewRegistrationDto dto)
        {
            var response = new Response<bool>();
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var registration = await _context.StudentRegistrations.FindAsync(dto.RegistrationId);
                    if (registration == null) { response.Success = false; response.Message = "Registration not found."; return response; }

                    // Update existing registration DueDate
                    registration.DueDate = registration.DueDate.AddMonths(dto.Months);
                    registration.Status = RegistrationStatus.Active; // Ensure it's active if it was expired

                    // Insert new payment history
                    var payment = new Payment
                    {
                        LibraryId = dto.LibraryId,
                        RegistrationId = dto.RegistrationId,
                        Amount = dto.Amount,
                        PaymentDate = DateTime.UtcNow,
                        NextDueDate = registration.DueDate,
                        PaymentMode = dto.PaymentMode,
                        Notes = dto.Notes,
                        CreatedBy = dto.CreatedBy
                    };
                    _context.Payments.Add(payment);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    response.Data = true; response.Success = true; response.Message = "Registration renewed.";
                }
                catch (Exception ex) 
                { 
                    await transaction.RollbackAsync();
                    response.Success = false; response.Message = ex.Message; 
                }
                return response;
            });
        }

        public async Task<Response<List<PaymentResponseDto>>> GetPaymentHistoryAsync(int registrationId)
        {
            var response = new Response<List<PaymentResponseDto>>();
            try
            {
                response.Data = await _context.Payments
                    .Include(p => p.CreatedByUser)
                    .Include(p => p.StudentRegistration).ThenInclude(r => r.Student)
                    .Where(p => p.RegistrationId == registrationId)
                    .OrderByDescending(p => p.PaymentDate)
                    .Select(p => new PaymentResponseDto
                    {
                        Id = p.Id, RegistrationId = p.RegistrationId, StudentName = p.StudentRegistration.Student.FullName,
                        Amount = p.Amount, PaymentDate = p.PaymentDate, NextDueDate = p.NextDueDate,
                        PaymentMode = p.PaymentMode, TransactionId = p.TransactionId, Notes = p.Notes,
                        CreatedBy = p.CreatedBy, CreatedByName = p.CreatedByUser.FullName
                    }).ToListAsync();
                response.Success = true;
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<SeatBatchStatusDto>> GetSeatAvailabilityAsync(int seatId, int libraryId, int? registrationId = null)
        {
            var response = new Response<SeatBatchStatusDto>();
            try
            {
                var batches = await _context.Batches
                    .Where(b => b.LibraryId == libraryId && b.IsActive)
                    .ToListAsync();

                var query = _context.StudentRegistrations
                    .Where(r => r.TableSeatId == seatId && r.Status == RegistrationStatus.Active);
                
                if (registrationId.HasValue && registrationId.Value > 0)
                {
                    query = query.Where(r => r.Id != registrationId.Value);
                }

                var activeRegistrations = await query.Select(r => r.BatchId).ToListAsync();

                var result = new SeatBatchStatusDto
                {
                    TableSeatId = seatId,
                    Batches = batches.Select(b => new BatchStatusDto
                    {
                        BatchId = b.Id,
                        BatchName = b.Name,
                        BatchTime = b.StartTime + " - " + b.EndTime,
                        IsOccupied = activeRegistrations.Contains(b.Id)
                    }).ToList()
                };

                response.Data = result; response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<PagedResult<StudentRegistrationListDto>>> GetDueStudentsAsync(SearchQuery query)
        {
            query.SearchTerm = "DUE"; // Hacky way to signal filter if needed, or just implement here
            var q = _context.StudentRegistrations.Include(r => r.Student).Include(r => r.TableSeat)
                    .Where(r => r.Status == RegistrationStatus.Active && r.DueDate < DateTime.UtcNow.AddDays(3));
            return await ExecutePagedListAsync(q, query);
        }

        public async Task<Response<PagedResult<StudentRegistrationListDto>>> GetTodayDueStudentsAsync(SearchQuery query)
        {
            var today = DateTime.UtcNow.Date;
            var q = _context.StudentRegistrations.Include(r => r.Student).Include(r => r.TableSeat)
                    .Where(r => r.Status == RegistrationStatus.Active && r.DueDate.Date == today);
            return await ExecutePagedListAsync(q, query);
        }

        public async Task<Response<PagedResult<StudentRegistrationListDto>>> GetExpiredStudentsAsync(SearchQuery query)
        {
            var q = _context.StudentRegistrations.Include(r => r.Student).Include(r => r.TableSeat)
                    .Where(r => r.Status == RegistrationStatus.Expired || (r.Status == RegistrationStatus.Active && r.DueDate < DateTime.UtcNow));
            return await ExecutePagedListAsync(q, query);
        }

        public async Task<Response<PagedResult<StudentRegistrationListDto>>> GetCancelledStudentsAsync(SearchQuery query)
        {
            var q = _context.StudentRegistrations.Include(r => r.Student).Include(r => r.TableSeat)
                    .Where(r => r.Status == RegistrationStatus.Cancelled);
            return await ExecutePagedListAsync(q, query);
        }

        private async Task<Response<PagedResult<StudentRegistrationListDto>>> ExecutePagedListAsync(IQueryable<StudentRegistration> q, SearchQuery query)
        {
            var response = new Response<PagedResult<StudentRegistrationListDto>>();
            if (query.LibraryId.HasValue) q = q.Where(r => r.Student.LibraryId == query.LibraryId.Value);
            
            var totalCount = await q.CountAsync();
            var items = await CommonQuery.ApplyPagination(q, query.PageNumber, query.PageSize)
                .Select(r => new StudentRegistrationListDto
                {
                    Id = r.Id, StudentId = r.StudentId, StudentName = r.Student.FullName, Mobile = r.Student.Mobile,
                    TableSeatId = r.TableSeatId, SeatNumber = r.TableSeat.SeatNumber, 
                    BatchId = r.BatchId, BatchName = r.Batch.Name,
                    StartDate = r.StartDate, DueDate = r.DueDate, MonthlyAmount = r.MonthlyAmount,
                    Status = r.Status.ToString()
                }).ToListAsync();

            response.Data = CommonQuery.BuildPagedResult(items, totalCount, query.PageNumber, query.PageSize);
            response.Success = true;
            return response;
        }

        private async Task<StudentRegistrationResponseDto?> BuildResponseAsync(int id) =>
            await _context.StudentRegistrations
                .Include(r => r.Student).Include(r => r.TableSeat).Include(r => r.Batch).Include(r => r.CreatedByUser)
                .Where(r => r.Id == id)
                .Select(r => new StudentRegistrationResponseDto
                {
                    Id = r.Id, LibraryId = r.LibraryId, StudentId = r.StudentId, StudentName = r.Student.FullName,
                    TableSeatId = r.TableSeatId, SeatNumber = r.TableSeat.SeatNumber, TableNumber = r.TableSeat.TableNumber,
                    BatchId = r.BatchId, BatchName = r.Batch.Name, BatchTime = r.Batch.StartTime + " - " + r.Batch.EndTime,
                    RegistrationDate = r.RegistrationDate, StartDate = r.StartDate, DueDate = r.DueDate,
                    MonthlyAmount = r.MonthlyAmount, SecurityAmount = r.SecurityAmount,
                    Notes = r.Notes, Status = r.Status.ToString(),
                    CreatedBy = r.CreatedBy, CreatedByName = r.CreatedByUser.FullName
                }).FirstOrDefaultAsync();
    }
}
