using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;

namespace Multi_Library_Management_Api.Repository
{
    public class BatchRepository : IBatchRepository
    {
        private readonly AppDbContext _context;
        public BatchRepository(AppDbContext context) => _context = context;

        public async Task<Response<BatchResponseDto>> CreateAsync(CreateBatchDto dto)
        {
            var response = new Response<BatchResponseDto>();
            try
            {
                var batch = new Batch
                {
                    LibraryId = dto.LibraryId,
                    Name = dto.Name,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Batches.Add(batch);
                await _context.SaveChangesAsync();
                response.Data = MapToDto(batch);
                response.Success = true;
                response.Message = "Batch created successfully.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<BatchResponseDto>> UpdateAsync(UpdateBatchDto dto)
        {
            var response = new Response<BatchResponseDto>();
            try
            {
                var batch = await _context.Batches.FindAsync(dto.Id);
                if (batch == null) { response.Success = false; response.Message = "Batch not found."; return response; }

                batch.Name = dto.Name;
                batch.StartTime = dto.StartTime;
                batch.EndTime = dto.EndTime;
                batch.IsActive = dto.IsActive;

                await _context.SaveChangesAsync();
                response.Data = MapToDto(batch);
                response.Success = true;
                response.Message = "Batch updated successfully.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<bool>> DeleteAsync(int id)
        {
            var response = new Response<bool>();
            try
            {
                var batch = await _context.Batches.FindAsync(id);
                if (batch == null) { response.Success = false; response.Message = "Batch not found."; return response; }
                batch.IsActive = false;
                await _context.SaveChangesAsync();
                response.Data = true; response.Success = true; response.Message = "Batch deactivated.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<BatchResponseDto>> GetByIdAsync(int id)
        {
            var response = new Response<BatchResponseDto>();
            try
            {
                var batch = await _context.Batches.FindAsync(id);
                if (batch == null) { response.Success = false; response.Message = "Batch not found."; return response; }
                response.Data = MapToDto(batch);
                response.Success = true;
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<List<BatchResponseDto>>> GetByLibraryIdAsync(int libraryId)
        {
            var response = new Response<List<BatchResponseDto>>();
            try
            {
                var batches = await _context.Batches
                    .Where(b => b.LibraryId == libraryId && b.IsActive)
                    .Select(b => new BatchResponseDto
                    {
                        Id = b.Id,
                        LibraryId = b.LibraryId,
                        Name = b.Name,
                        StartTime = b.StartTime,
                        EndTime = b.EndTime,
                        IsActive = b.IsActive,
                        CreatedDate = b.CreatedDate
                    })
                    .ToListAsync();
                response.Data = batches;
                response.Success = true;
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        private static BatchResponseDto MapToDto(Batch batch)
        {
            return new BatchResponseDto
            {
                Id = batch.Id,
                LibraryId = batch.LibraryId,
                Name = batch.Name,
                StartTime = batch.StartTime,
                EndTime = batch.EndTime,
                IsActive = batch.IsActive,
                CreatedDate = batch.CreatedDate
            };
        }
    }
}
