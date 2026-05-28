using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;

        public StudentRepository(AppDbContext context, IWebHostEnvironment env, IEmailService emailService)
        {
            _context = context;
            _env = env;
            _emailService = emailService;
        }

        private async Task<string?> SaveFileAsync(IFormFile? file, string subFolder)
        {
            if (file == null || file.Length == 0) return null;

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads", subFolder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine("uploads", subFolder, fileName).Replace("\\", "/");
        }

        public async Task<Response<StudentResponseDto>> CreateAsync(CreateStudentDto dto)
        {
            var response = new Response<StudentResponseDto>();
            try
            {
                var photoPath = await SaveFileAsync(dto.StudentImage, "student");
                var docImagePath = await SaveFileAsync(dto.DocumentImage, "student");

                var student = new Student
                {
                    LibraryId = dto.LibraryId,
                    FullName = dto.FullName,
                    FatherName = dto.FatherName,
                    Mobile = dto.Mobile,
                    Email = dto.Email,
                    Address = dto.Address,
                    Gender = dto.Gender,
                    Photo = photoPath,
                    DocumentImage = docImagePath,
                    DocumentType = dto.DocumentType,
                    DOB = dto.DOB,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                // Send Registration Email with Virtual Card
                if (!string.IsNullOrEmpty(student.Email))
                {
                    var library = await _context.Libraries.FindAsync(student.LibraryId);
                    string photoUrl = !string.IsNullOrEmpty(student.Photo) ? student.Photo : "";
                    
                    string subject = $"Welcome to {library?.Name} - Enrollment ID";
                    string body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                        <h2 style='color: #2c3e50;'>Welcome to the Library!</h2>
                        <p>Dear {student.FullName},</p>
                        <p>Thank you for joining <b>{library?.Name}</b>. Your basic enrollment is complete.</p>
                        <p style='font-size: 18px;'><b>Your Enrollment ID: {student.Id}</b></p>
                        <p>Please use this ID when you visit the library to complete your seat registration and payment.</p>
                        <br/>
                        <p>Regards,<br/>{library?.Name} Team</p>
                    </div>";

                    await _emailService.SendEmailAsync(student.Email, subject, body, student.LibraryId);
                }

                response.Data = await BuildResponseAsync(student.Id);
                response.Success = true; response.Message = "Student created.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<StudentResponseDto>> UpdateAsync(UpdateStudentDto dto)
        {
            var response = new Response<StudentResponseDto>();
            try
            {
                var student = await _context.Students.FindAsync(dto.Id);
                if (student == null) { response.Success = false; response.Message = "Student not found."; return response; }

                if (dto.StudentImage != null)
                {
                    student.Photo = await SaveFileAsync(dto.StudentImage, "student");
                }
                if (dto.DocumentImage != null)
                {
                    student.DocumentImage = await SaveFileAsync(dto.DocumentImage, "student");
                }

                student.FullName = dto.FullName;
                student.FatherName = dto.FatherName;
                student.Mobile = dto.Mobile;
                student.Email = dto.Email;
                student.Address = dto.Address;
                student.Gender = dto.Gender;
                student.DocumentType = dto.DocumentType;
                student.DOB = dto.DOB;
                student.IsActive = dto.IsActive;

                await _context.SaveChangesAsync();
                response.Data = await BuildResponseAsync(student.Id);
                response.Success = true; response.Message = "Student updated.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<bool>> DeleteAsync(int id)
        {
            var response = new Response<bool>();
            try
            {
                var student = await _context.Students.FindAsync(id);
                if (student == null) { response.Success = false; response.Message = "Student not found."; return response; }
                student.IsActive = false;
                await _context.SaveChangesAsync();
                response.Data = true; response.Success = true; response.Message = "Student deactivated.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<StudentResponseDto>> GetByIdAsync(int id)
        {
            var response = new Response<StudentResponseDto>();
            try
            {
                var dto = await BuildResponseAsync(id);
                if (dto == null) { response.Success = false; response.Message = "Student not found."; return response; }
                response.Data = dto; response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<PagedResult<StudentListDto>>> GetAllAsync(SearchQuery query)
        {
            var response = new Response<PagedResult<StudentListDto>>();
            try
            {
                var q = _context.Students.Include(s => s.Library).AsQueryable();

                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                    q = q.Where(s => s.FullName.Contains(query.SearchTerm) || s.Mobile.Contains(query.SearchTerm));
                if (query.IsActive.HasValue) q = q.Where(s => s.IsActive == query.IsActive.Value);
                if (query.LibraryId.HasValue) q = q.Where(s => s.LibraryId == query.LibraryId.Value);

                var totalCount = await q.CountAsync();
                var items = await CommonQuery.ApplyPagination(q, query.PageNumber, query.PageSize)
                    .Select(s => new StudentListDto
                    {
                        Id = s.Id, FullName = s.FullName, FatherName = s.FatherName, 
                        Mobile = s.Mobile, Email = s.Email, Address = s.Address, Gender = s.Gender,
                        LibraryName = s.Library.Name, Photo = s.Photo, 
                        DocumentImage = s.DocumentImage, DocumentType = s.DocumentType, 
                        DOB = s.DOB, IsActive = s.IsActive
                    }).ToListAsync();

                response.Data = CommonQuery.BuildPagedResult(items, totalCount, query.PageNumber, query.PageSize);
                response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        private async Task<StudentResponseDto?> BuildResponseAsync(int id) =>
            await _context.Students.Include(s => s.Library).Where(s => s.Id == id)
                .Select(s => new StudentResponseDto
                {
                    Id = s.Id, LibraryId = s.LibraryId, LibraryName = s.Library.Name,
                    FullName = s.FullName, FatherName = s.FatherName, Mobile = s.Mobile,
                    Email = s.Email, Address = s.Address, Gender = s.Gender, Photo = s.Photo,
                    DocumentImage = s.DocumentImage, DocumentType = s.DocumentType,
                    DOB = s.DOB, IsActive = s.IsActive, CreatedDate = s.CreatedDate
                }).FirstOrDefaultAsync();
    }
}
