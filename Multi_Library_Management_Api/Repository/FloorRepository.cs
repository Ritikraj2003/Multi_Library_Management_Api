using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Repository
{
    public class FloorRepository : IFloorRepository
    {
        private readonly AppDbContext _context;
        public FloorRepository(AppDbContext context) => _context = context;

        public async Task<Response<FloorResponseDto>> CreateAsync(CreateFloorDto dto)
        {
            var response = new Response<FloorResponseDto>();
            try
            {
                var floor = new Floor
                {
                    LibraryId = dto.LibraryId, Name = dto.Name,
                    FloorNumber = dto.FloorNumber, IsActive = true
                };
                _context.Floors.Add(floor);
                await _context.SaveChangesAsync();
                response.Data = await BuildResponseAsync(floor.Id);
                response.Success = true; response.Message = "Floor created.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<FloorResponseDto>> UpdateAsync(UpdateFloorDto dto)
        {
            var response = new Response<FloorResponseDto>();
            try
            {
                var floor = await _context.Floors.FindAsync(dto.Id);
                if (floor == null) { response.Success = false; response.Message = "Floor not found."; return response; }
                floor.Name = dto.Name; floor.FloorNumber = dto.FloorNumber; floor.IsActive = dto.IsActive;
                await _context.SaveChangesAsync();
                response.Data = await BuildResponseAsync(floor.Id);
                response.Success = true; response.Message = "Floor updated.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<bool>> DeleteAsync(int id)
        {
            var response = new Response<bool>();
            try
            {
                var floor = await _context.Floors.FindAsync(id);
                if (floor == null) { response.Success = false; response.Message = "Floor not found."; return response; }
                floor.IsActive = false;
                await _context.SaveChangesAsync();
                response.Data = true; response.Success = true; response.Message = "Floor deactivated.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<FloorResponseDto>> GetByIdAsync(int id)
        {
            var response = new Response<FloorResponseDto>();
            try
            {
                var dto = await BuildResponseAsync(id);
                if (dto == null) { response.Success = false; response.Message = "Floor not found."; return response; }
                response.Data = dto; response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<PagedResult<FloorListDto>>> GetAllAsync(SearchQuery query)
        {
            var response = new Response<PagedResult<FloorListDto>>();
            try
            {
                var q = _context.Floors.Include(f => f.Library).AsQueryable();

                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                    q = q.Where(f => f.Name.Contains(query.SearchTerm));
                if (query.IsActive.HasValue) q = q.Where(f => f.IsActive == query.IsActive.Value);
                if (query.LibraryId.HasValue) q = q.Where(f => f.LibraryId == query.LibraryId.Value);

                var totalCount = await q.CountAsync();
                var items = await CommonQuery.ApplyPagination(q, query.PageNumber, query.PageSize)
                    .Select(f => new FloorListDto
                    {
                        Id = f.Id, Name = f.Name, FloorNumber = f.FloorNumber,
                        LibraryName = f.Library.Name, IsActive = f.IsActive
                    }).ToListAsync();

                response.Data = CommonQuery.BuildPagedResult(items, totalCount, query.PageNumber, query.PageSize);
                response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        private async Task<FloorResponseDto?> BuildResponseAsync(int id) =>
            await _context.Floors.Include(f => f.Library).Where(f => f.Id == id)
                .Select(f => new FloorResponseDto
                {
                    Id = f.Id, LibraryId = f.LibraryId, LibraryName = f.Library.Name,
                    Name = f.Name, FloorNumber = f.FloorNumber, IsActive = f.IsActive
                }).FirstOrDefaultAsync();
    }
}
