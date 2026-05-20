using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Multi_Library_Management_Api.Repository
{
    public class StudentRegistrationRepository : IStudentRegistrationRepository
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public StudentRegistrationRepository(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

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

                    var requestedBatch = await _context.Batches.FindAsync(dto.BatchId);
                    if (requestedBatch == null) { response.Success = false; response.Message = "Batch not found."; return response; }

                    var activeRegistrations = await _context.StudentRegistrations
                        .Include(r => r.Batch)
                        .Where(r => r.TableSeatId == dto.TableSeatId && r.Status == RegistrationStatus.Active)
                        .ToListAsync();
                    
                    bool isTimeOccupied = activeRegistrations.Any(r => 
                        TimeOverlapHelper.IsOverlapping(requestedBatch.StartTime, requestedBatch.EndTime, r.Batch.StartTime, r.Batch.EndTime));

                    if (isTimeOccupied) { response.Success = false; response.Message = "Seat is already occupied during this time range."; return response; }

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
                        Notes = dto.Notes,
                        RFIDCode = dto.RFIDCode,
                        Status = RegistrationStatus.Active,
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
                    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var regDto = await BuildResponseAsync(registration.Id);
                    response.Data = regDto;
                    response.Success = true; response.Message = "Student registered successfully.";

                    // Send Registration Email with Virtual Card
                    if (regDto != null && !string.IsNullOrEmpty(student.Email))
                    {
                        var library = await _context.Libraries.FindAsync(dto.LibraryId);
                        string photoUrl = !string.IsNullOrEmpty(student.Photo) ? student.Photo : "";
                        
                        string subject = $"Registration Successful - {library?.Name}";
                        
                        // Card HTML for attachment and body
                        string cardHtml = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 500px; margin: auto; padding: 20px;'>
                            <div style='background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%); color: white; padding: 30px; border-radius: 20px; box-shadow: 0 15px 35px rgba(30,60,114,0.3); position: relative; overflow: hidden;'>
                                <div style='position: absolute; top: -50px; right: -50px; width: 150px; height: 150px; background: rgba(255,255,255,0.1); border-radius: 50%;'></div>
                                
                                <div style='display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 25px;'>
                                    <div>
                                        <h2 style='margin: 0; font-size: 22px; letter-spacing: 1px;'>LIBRARY ID CARD</h2>
                                        <p style='margin: 5px 0 0 0; opacity: 0.9; font-size: 14px;'>{library?.Name}</p>
                                    </div>
                                    <div style='background: #e67e22; padding: 6px 12px; border-radius: 8px; font-size: 11px; font-weight: bold;'>ACTIVE</div>
                                </div>

                                <div style='display: flex; gap: 25px; align-items: center;'>
                                    <div style='width: 110px; height: 110px; background: white; border-radius: 15px; padding: 4px; box-shadow: 0 5px 15px rgba(0,0,0,0.2);'>
                                        <img src='{(string.IsNullOrEmpty(photoUrl) ? "https://cdn-icons-png.flaticon.com/512/3135/3135715.png" : "https://yourdomain.com/" + photoUrl)}' 
                                             style='width: 100%; height: 100%; object-fit: cover; border-radius: 12px;'>
                                    </div>
                                    <div style='flex: 1;'>
                                        <p style='margin: 0; font-size: 20px; font-weight: bold;'>{student.FullName}</p>
                                        <div style='margin-top: 10px; display: grid; grid-template-columns: 1fr 1fr; gap: 10px;'>
                                            <div>
                                                <p style='margin: 0; font-size: 9px; text-transform: uppercase; opacity: 0.7;'>Seat No</p>
                                                <p style='margin: 1px 0 0 0; font-size: 13px; font-weight: bold;'>{regDto.TableNumber}-{regDto.SeatNumber}</p>
                                            </div>
                                            <div>
                                                <p style='margin: 0; font-size: 9px; text-transform: uppercase; opacity: 0.7;'>Batch</p>
                                                <p style='margin: 1px 0 0 0; font-size: 13px; font-weight: bold;'>{regDto.BatchName}</p>
                                            </div>
                                        </div>
                                        <div style='margin-top: 10px;'>
                                            <p style='margin: 0; font-size: 9px; text-transform: uppercase; opacity: 0.7;'>Valid Till</p>
                                            <p style='margin: 1px 0 0 0; font-size: 13px; font-weight: bold; color: #f1c40f;'>{regDto.DueDate:dd MMM yyyy}</p>
                                        </div>
                                    </div>
                                </div>
                                
                                <div style='margin-top: 25px; border-top: 1px solid rgba(255,255,255,0.2); padding-top: 15px; display: flex; justify-content: space-between; align-items: center;'>
                                    <p style='margin: 0; font-size: 12px; letter-spacing: 2px;'>ID: {student.Id.ToString("D5")}</p>
                                    <div style='background: white; padding: 5px; border-radius: 5px;'>
                                        <img src='https://api.qrserver.com/v1/create-qr-code/?size=40x40&data={student.Id}' style='display: block;'>
                                    </div>
                                </div>
                            </div>
                        </div>";

                        string body = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px;'>
                            <h2 style='color: #2c3e50;'>Registration Confirmed!</h2>
                            <p>Dear {student.FullName},</p>
                            <p>Your seat registration for <b>{library?.Name}</b> is successful. Please find your virtual ID card attached to this email.</p>
                            
                            {cardHtml}

                            <div style='margin-top: 25px; background: #f9f9f9; padding: 15px; border-radius: 12px; border: 1px solid #eee;'>
                                <h4 style='margin: 0 0 10px 0;'>Payment Details</h4>
                                <p style='margin: 5px 0;'>Amount Paid: <b>₹{dto.MonthlyAmount}</b></p>
                                <p style='margin: 5px 0;'>Next Due Date: <b style='color: #e74c3c;'>{dto.DueDate:dd MMM yyyy}</b></p>
                            </div>
                        </div>";
                        
                        byte[] attachmentData = System.Text.Encoding.UTF8.GetBytes(cardHtml);
                        await _emailService.SendEmailAsync(student.Email, subject, body, dto.LibraryId, attachmentData, "LibraryCard.html");
                    }
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
                registration.RFIDCode = dto.RFIDCode;

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
                {
                    var term = query.SearchTerm.Trim().ToLower();
                    int.TryParse(term, out var idSearch);
                    q = q.Where(r =>
                        r.Student.FullName.ToLower().Contains(term) ||
                        r.Student.Mobile.Contains(term) ||
                        (idSearch > 0 && r.Id == idSearch));
                }
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
                        RFIDCode = r.RFIDCode,
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
                    .Include(r => r.Batch)
                    .Where(r => r.TableSeatId == seatId && r.Status == RegistrationStatus.Active);
                
                if (registrationId.HasValue && registrationId.Value > 0)
                {
                    query = query.Where(r => r.Id != registrationId.Value);
                }

                var activeRegs = await query.ToListAsync();

                var result = new SeatBatchStatusDto
                {
                    TableSeatId = seatId,
                    Batches = batches.Select(b => {
                        bool isDirectlyOccupied = activeRegs.Any(r => r.BatchId == b.Id);
                        bool isOverlappinglyOccupied = activeRegs.Any(r => 
                            TimeOverlapHelper.IsOverlapping(b.StartTime, b.EndTime, r.Batch.StartTime, r.Batch.EndTime));

                        return new BatchStatusDto
                        {
                            BatchId = b.Id,
                            BatchName = b.Name,
                            BatchTime = b.StartTime + " - " + b.EndTime,
                            StartTime = b.StartTime,
                            EndTime = b.EndTime,
                            IsOccupied = isDirectlyOccupied || isOverlappinglyOccupied,
                            IsDirectlyOccupied = isDirectlyOccupied
                        };
                    }).ToList()
                };

                response.Data = result; response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        private static class TimeOverlapHelper
        {
            public static bool IsOverlapping(string start1, string end1, string start2, string end2)
            {
                if (string.IsNullOrEmpty(start1) || string.IsNullOrEmpty(end1)) return false;
                if (string.IsNullOrEmpty(start2) || string.IsNullOrEmpty(end2)) return false;

                int s1 = TimeToMinutes(start1);
                int e1 = TimeToMinutes(end1);
                int s2 = TimeToMinutes(start2);
                int e2 = TimeToMinutes(end2);

                if (s1 == 0 && e1 == 0) return false;
                if (s2 == 0 && e2 == 0) return false;

                if (e1 <= s1) e1 += 1440;
                if (e2 <= s2) e2 += 1440;

                return s1 < e2 && s2 < e1;
            }

            private static int TimeToMinutes(string time)
            {
                if (string.IsNullOrEmpty(time)) return 0;
                var parts = time.Split(':');
                if (parts.Length < 2) return 0;
                if (int.TryParse(parts[0], out int h) && int.TryParse(parts[1], out int m))
                    return h * 60 + m;
                return 0;
            }
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

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.Trim().ToLower();
                int.TryParse(term, out var idSearch);
                q = q.Where(r =>
                    r.Student.FullName.ToLower().Contains(term) ||
                    r.Student.Mobile.Contains(term) ||
                    (idSearch > 0 && r.Id == idSearch));
            }
            
            var totalCount = await q.CountAsync();
            var items = await CommonQuery.ApplyPagination(q, query.PageNumber, query.PageSize)
                .Select(r => new StudentRegistrationListDto
                {
                    Id = r.Id, StudentId = r.StudentId, StudentName = r.Student.FullName, Mobile = r.Student.Mobile,
                    TableSeatId = r.TableSeatId, SeatNumber = r.TableSeat.SeatNumber, 
                    BatchId = r.BatchId, BatchName = r.Batch.Name,
                    StartDate = r.StartDate, DueDate = r.DueDate, MonthlyAmount = r.MonthlyAmount,
                    RFIDCode = r.RFIDCode,
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
                    Id = r.Id, LibraryId = r.LibraryId, StudentId = r.StudentId, StudentName = r.Student.FullName, Mobile = r.Student.Mobile,
                    TableSeatId = r.TableSeatId, SeatNumber = r.TableSeat.SeatNumber, TableNumber = r.TableSeat.TableNumber,
                    BatchId = r.BatchId, BatchName = r.Batch.Name, BatchTime = r.Batch.StartTime + " - " + r.Batch.EndTime,
                    RegistrationDate = r.RegistrationDate, StartDate = r.StartDate, DueDate = r.DueDate,
                    MonthlyAmount = r.MonthlyAmount, SecurityAmount = r.SecurityAmount,
                    Notes = r.Notes, RFIDCode = r.RFIDCode, Status = r.Status.ToString(),
                    CreatedBy = r.CreatedBy, CreatedByName = r.CreatedByUser.FullName
                }).FirstOrDefaultAsync();

        public async Task<Response<bool>> SendReceiptEmailAsync(SendReceiptEmailDto dto)
        {
            var response = new Response<bool>();
            try
            {
                // 1. Fetch registration details
                var registration = await _context.StudentRegistrations
                    .Include(r => r.Student)
                    .Include(r => r.TableSeat)
                    .Include(r => r.Batch)
                    .Where(r => r.Id == dto.RegistrationId)
                    .FirstOrDefaultAsync();

                if (registration == null)
                {
                    response.Success = false;
                    response.Message = "Registration not found.";
                    return response;
                }

                // Query correct payment mode from payments history
                var paymentMode = await _context.Payments
                    .Where(p => p.RegistrationId == registration.Id)
                    .OrderByDescending(p => p.PaymentDate)
                    .Select(p => p.PaymentMode)
                    .FirstOrDefaultAsync() ?? "Cash";

                // 2. Fetch configured library name
                var libraryNameSetting = await _context.GeneralSettings
                    .Where(s => s.LibraryId == registration.LibraryId && s.Key == "library_name")
                    .Select(s => s.Value)
                    .FirstOrDefaultAsync();
                var libraryName = libraryNameSetting ?? "MBR Library";

                // 3. Set up email fields
                var recipientEmail = !string.IsNullOrEmpty(dto.CustomEmail) ? dto.CustomEmail : registration.Student.Email;
                if (string.IsNullOrEmpty(recipientEmail))
                {
                    response.Success = false;
                    response.Message = "Recipient email address not found.";
                    return response;
                }

                // 3. Fetch specific or latest payment details
                var payment = dto.PaymentId.HasValue 
                    ? await _context.Payments.FirstOrDefaultAsync(p => p.Id == dto.PaymentId.Value && p.RegistrationId == registration.Id)
                    : null;

                if (payment == null)
                {
                    payment = await _context.Payments
                        .Where(p => p.RegistrationId == registration.Id)
                        .OrderByDescending(p => p.Id)
                        .FirstOrDefaultAsync();
                }

                int paymentId = payment?.Id ?? registration.Id;
                decimal paymentAmount = payment?.Amount ?? registration.MonthlyAmount;
                DateTime nextDueDate = payment?.NextDueDate ?? registration.DueDate;

                var firstPaymentId = await _context.Payments
                    .Where(p => p.RegistrationId == registration.Id)
                    .OrderBy(p => p.Id)
                    .Select(p => p.Id)
                    .FirstOrDefaultAsync();

                bool isFirstPayment = payment != null && payment.Id == firstPaymentId;
                decimal securityAmount = isFirstPayment ? registration.SecurityAmount : 0;
                decimal totalAmount = paymentAmount + securityAmount;

                var receiptNo = $"SLM-REG-{paymentId}";
                var subject = $"Payment Confirmation Receipt - #{receiptNo} - {libraryName}";
                
                var frontendUrl = "http://localhost:4200"; // Can be moved to appsettings later if needed
                var receiptUrl = $"{frontendUrl}/receipt/{registration.Id}/{registration.LibraryId}/{paymentId}";

                var body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; line-height: 1.6; color: #333;'>
                        <h2 style='color: #0078d4;'>Payment Confirmation Receipt</h2>
                        <p>Dear <b>{registration.Student.FullName}</b>,</p>
                        <p>Thank you for your seat registration payment at <b>{libraryName}</b>. We have successfully processed your transaction.</p>
                        <p>Please find your receipt summary below and click the button to view or download your full bill.</p>
                        
                        <div style='background: #f4f6f9; padding: 15px; border-radius: 8px; border: 1px solid #e0e0e0; margin: 20px 0;'>
                            <p style='margin: 5px 0;'><b>Receipt Number:</b> #{receiptNo}</p>
                            <p style='margin: 5px 0;'><b>Allocated Seat:</b> Seat {registration.TableSeat.SeatNumber}</p>
                            <p style='margin: 5px 0;'><b>Shift/Batch:</b> {registration.Batch.Name}</p>
                            <p style='margin: 5px 0;'><b>Amount Paid:</b> ₹{totalAmount:F2}</p>
                            <p style='margin: 5px 0;'><b>Next Due Date:</b> {nextDueDate:dd MMM yyyy}</p>
                        </div>

                        <div style='margin: 30px 0;'>
                            <a href='{receiptUrl}' style='background-color: #0078d4; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>View & Download Bill</a>
                        </div>

                        <p>Best regards,<br>Management Team<br><b>{libraryName}</b></p>
                    </div>";

                // 4. Send email without the PDF attachment
                await _emailService.SendEmailAsync(
                    recipientEmail,
                    subject,
                    body,
                    registration.LibraryId
                );

                response.Data = true;
                response.Success = true;
                response.Message = "Email sent successfully with link to receipt.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<Response<StudentRegistrationResponseDto>> GetPublicReceiptAsync(int registrationId, int libraryId, int paymentId)
        {
            var response = new Response<StudentRegistrationResponseDto>();
            try
            {
                var registration = await _context.StudentRegistrations
                    .Include(r => r.Student)
                    .Include(r => r.TableSeat)
                    .Include(r => r.Batch)
                    .Include(r => r.CreatedByUser)
                    .Where(r => r.Id == registrationId && r.LibraryId == libraryId)
                    .FirstOrDefaultAsync();

                if (registration == null)
                {
                    response.Success = false;
                    response.Message = "Registration not found.";
                    return response;
                }

                var payment = await _context.Payments
                    .Include(p => p.CreatedByUser)
                    .Where(p => p.Id == paymentId && p.RegistrationId == registrationId && p.LibraryId == libraryId)
                    .FirstOrDefaultAsync();

                if (payment == null)
                {
                    payment = await _context.Payments
                        .Include(p => p.CreatedByUser)
                        .Where(p => p.RegistrationId == registrationId && p.LibraryId == libraryId)
                        .OrderByDescending(p => p.Id)
                        .FirstOrDefaultAsync();
                }

                if (payment == null)
                {
                    response.Success = false;
                    response.Message = "Payment receipt not found.";
                    return response;
                }

                var firstPaymentId = await _context.Payments
                    .Where(p => p.RegistrationId == registrationId)
                    .OrderBy(p => p.Id)
                    .Select(p => p.Id)
                    .FirstOrDefaultAsync();

                bool isFirstPayment = payment.Id == firstPaymentId;
                decimal securityAmount = isFirstPayment ? registration.SecurityAmount : 0;

                var dto = new StudentRegistrationResponseDto
                {
                    Id = payment.Id, // Display receipt number as payment ID
                    RegistrationId = registration.Id, // Original registration ID
                    LibraryId = registration.LibraryId,
                    StudentId = registration.StudentId,
                    StudentName = registration.Student.FullName,
                    Mobile = registration.Student.Mobile,
                    TableSeatId = registration.TableSeatId,
                    SeatNumber = registration.TableSeat.SeatNumber,
                    TableNumber = registration.TableSeat.TableNumber,
                    BatchId = registration.BatchId,
                    BatchName = registration.Batch.Name,
                    BatchTime = registration.Batch.StartTime + " - " + registration.Batch.EndTime,
                    RegistrationDate = payment.PaymentDate,
                    StartDate = registration.StartDate,
                    DueDate = payment.NextDueDate,
                    MonthlyAmount = payment.Amount,
                    SecurityAmount = securityAmount,
                    Notes = payment.Notes,
                    RFIDCode = registration.RFIDCode,
                    Status = registration.Status.ToString(),
                    CreatedBy = payment.CreatedBy,
                    CreatedByName = payment.CreatedByUser?.FullName ?? registration.CreatedByUser?.FullName ?? "Administrator",
                    PaymentMode = payment.PaymentMode
                };

                response.Data = dto;
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
    }
}
