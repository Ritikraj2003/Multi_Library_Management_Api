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

        public StudentRepository(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
                    Address = dto.Address,
                    RFIDCode = dto.RFIDCode,
                    Photo = photoPath,
                    DocumentImage = docImagePath,
                    DocumentType = dto.DocumentType,
                    DOB = dto.DOB,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
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
                student.Address = dto.Address;
                student.RFIDCode = dto.RFIDCode;
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
                        Id = s.Id, FullName = s.FullName, FatherName = s.FatherName, Mobile = s.Mobile,
                        LibraryName = s.Library.Name, RFIDCode = s.RFIDCode, Photo = s.Photo, 
                        DocumentImage = s.DocumentImage, DocumentType = s.DocumentType, IsActive = s.IsActive
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
                    Address = s.Address, RFIDCode = s.RFIDCode, Photo = s.Photo,
                    DocumentImage = s.DocumentImage, DocumentType = s.DocumentType,
                    DOB = s.DOB, IsActive = s.IsActive, CreatedDate = s.CreatedDate
                }).FirstOrDefaultAsync();
    }
}
