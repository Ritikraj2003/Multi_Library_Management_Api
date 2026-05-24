using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;
using Multi_Library_Management_Api.Query;

namespace Multi_Library_Management_Api.Repository
{
    public class TableSeatRepository : ITableSeatRepository
    {
        private readonly AppDbContext _context;
        public TableSeatRepository(AppDbContext context) => _context = context;

        public async Task<Response<TableSeatResponseDto>> CreateAsync(CreateTableSeatDto dto)
        {
            var response = new Response<TableSeatResponseDto>();
            try
            {
                var seat = new TableSeat
                {
                    LibraryId = dto.LibraryId,
                    FloorId = dto.FloorId,
                    TableNumber = dto.TableNumber,
                    SeatNumber = dto.SeatNumber,
                    IsOccupied = false,
                    IsActive = true,
                    XAxis = dto.XAxis,
                    YAxis = dto.YAxis
                };
                _context.TableSeats.Add(seat);
                await _context.SaveChangesAsync();
                response.Data = await BuildResponseAsync(seat.Id);
                response.Success = true; response.Message = "Seat created.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<TableSeatResponseDto>> UpdateAsync(UpdateTableSeatDto dto)
        {
            var response = new Response<TableSeatResponseDto>();
            try
            {
                var seat = await _context.TableSeats.FindAsync(dto.Id);
                if (seat == null) { response.Success = false; response.Message = "Seat not found."; return response; }
                seat.LibraryId = dto.LibraryId;
                seat.TableNumber = dto.TableNumber;
                seat.SeatNumber = dto.SeatNumber;
                seat.IsActive = dto.IsActive;
                seat.XAxis = dto.XAxis;
                seat.YAxis = dto.YAxis;
                await _context.SaveChangesAsync();
                response.Data = await BuildResponseAsync(seat.Id);
                response.Success = true; response.Message = "Seat updated.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<bool>> DeleteAsync(int id)
        {
            var response = new Response<bool>();
            try
            {
                var seat = await _context.TableSeats.FindAsync(id);
                if (seat == null) { response.Success = false; response.Message = "Seat not found."; return response; }
                if (seat.IsOccupied)
                {
                    response.Success = false; response.Message = "Cannot delete an occupied seat."; return response;
                }
                seat.IsActive = false;
                await _context.SaveChangesAsync();
                response.Data = true; response.Success = true; response.Message = "Seat deactivated.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<TableSeatResponseDto>> GetByIdAsync(int id)
        {
            var response = new Response<TableSeatResponseDto>();
            try
            {
                var dto = await BuildResponseAsync(id);
                if (dto == null) { response.Success = false; response.Message = "Seat not found."; return response; }
                response.Data = dto; response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<PagedResult<TableSeatListDto>>> GetAllAsync(SearchQuery query)
        {
            var response = new Response<PagedResult<TableSeatListDto>>();
            try
            {
                var q = _context.TableSeats.Include(ts => ts.Floor).Include(ts => ts.Library).AsQueryable();

                if (query.LibraryId.HasValue && query.LibraryId.Value > 0)
                    q = q.Where(ts => ts.LibraryId == query.LibraryId.Value);

                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                    q = q.Where(ts => ts.SeatNumber.Contains(query.SearchTerm) ||
                                      ts.TableNumber.Contains(query.SearchTerm));
                if (query.IsActive.HasValue) q = q.Where(ts => ts.IsActive == query.IsActive.Value);

                var totalCount = await q.CountAsync();
                var items = await CommonQuery.ApplyPagination(q, query.PageNumber, query.PageSize)
                    .Select(ts => new TableSeatListDto
                    {
                        Id = ts.Id,
                        TableNumber = ts.TableNumber,
                        SeatNumber = ts.SeatNumber,
                        FloorId = ts.FloorId,
                        FloorName = ts.Floor.Name,
                        LibraryName = ts.Library.Name,
                        IsOccupied = ts.IsOccupied,
                        IsActive = ts.IsActive,
                        XAxis = ts.XAxis,
                        YAxis = ts.YAxis
                    }).ToListAsync();

                response.Data = CommonQuery.BuildPagedResult(items, totalCount, query.PageNumber, query.PageSize);
                response.Success = true; response.Message = "Success";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<bool>> BulkUpdatePositionsAsync(List<UpdateTableSeatPositionDto> dtos)
        {
            var response = new Response<bool>();
            try
            {
                var seatIds = dtos.Select(d => d.Id).ToList();
                var seats = await _context.TableSeats.Where(s => seatIds.Contains(s.Id)).ToListAsync();
                foreach (var dto in dtos)
                {
                    var seat = seats.FirstOrDefault(s => s.Id == dto.Id);
                    if (seat != null)
                    {
                        seat.XAxis = dto.XAxis;
                        seat.YAxis = dto.YAxis;
                    }
                }
                await _context.SaveChangesAsync();
                response.Data = true;
                response.Success = true;
                response.Message = "Positions updated.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        private async Task<TableSeatResponseDto?> BuildResponseAsync(int id) =>
            await _context.TableSeats.Include(ts => ts.Floor).Include(ts => ts.Library).Where(ts => ts.Id == id)
                .Select(ts => new TableSeatResponseDto
                {
                    Id = ts.Id, LibraryId = ts.LibraryId, LibraryName = ts.Library.Name,
                    FloorId = ts.FloorId, FloorName = ts.Floor.Name,
                    TableNumber = ts.TableNumber, SeatNumber = ts.SeatNumber,
                    IsOccupied = ts.IsOccupied, IsActive = ts.IsActive,
                    XAxis = ts.XAxis, YAxis = ts.YAxis
                }).FirstOrDefaultAsync();
    }
}
