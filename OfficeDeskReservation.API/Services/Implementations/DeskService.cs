using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Dtos.Desks;
using OfficeDeskReservation.API.Models;
using OfficeDeskReservation.API.Pagination;
using OfficeDeskReservation.API.Services.Interfaces;

namespace OfficeDeskReservation.API.Services.Implementations
{
    public class DeskService : IDeskService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public DeskService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedResult<DeskResponseDto>> GetDesksAsync(DeskQueryParameters queryParameters)
        {
            var query = _context.Desks
                .Include(d => d.Room)
                .Include(d => d.Reservations)
                    .ThenInclude(r => r.User)
                .AsQueryable();

            if (queryParameters.RoomId.HasValue)
                query = query.Where(d => d.RoomId == queryParameters.RoomId);

            int totalCount = await query.CountAsync();

            List<Desk> desks = await query
                .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .ToListAsync();

            return new PagedResult<DeskResponseDto>
            {
                Items = _mapper.Map<List<DeskResponseDto>>(desks),
                TotalCount = totalCount,
                PageNumber = queryParameters.PageNumber,
                PageSize = queryParameters.PageSize
            };
        }

        public async Task<DeskResponseDto?> GetDeskByIdAsync(int id)
        {
            Desk? desk = await _context.Desks
                .Include(d => d.Room)
                .Include(d => d.Reservations)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            return desk == null ? null : _mapper.Map<DeskResponseDto>(desk);
        }

        public async Task<DeskResponseDto?> CreateDeskAsync(DeskDto desk)
        {
            if (await _context.Desks.AnyAsync(d => d.DeskIdentifier == desk.DeskIdentifier))
                throw new InvalidOperationException("The desk with that name already exists.");

            Room? existingRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == desk.RoomId);
            if (existingRoom == null)
                throw new KeyNotFoundException("The specified room doesn't exist.");

            Desk newDesk = _mapper.Map<Desk>(desk);
            newDesk.Room = existingRoom;

            _context.Desks.Add(newDesk);
            await _context.SaveChangesAsync();

            return _mapper.Map<DeskResponseDto>(newDesk);
        }

        public async Task<bool> UpdateDeskAsync(int id, DeskDto desk)
        {
            Desk? existingDesk = await _context.Desks.Include(d => d.Reservations).FirstOrDefaultAsync(d => d.Id == id);

            if (existingDesk == null)
                return false;

            if (await _context.Desks.AnyAsync(d => d.DeskIdentifier == desk.DeskIdentifier && d.Id != existingDesk.Id))
                throw new InvalidOperationException("The desk with that name already exists.");

            if (!await _context.Rooms.AnyAsync(r => r.Id == desk.RoomId))
                throw new KeyNotFoundException("The specified room doesn't exist.");

            _mapper.Map(desk, existingDesk);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteDeskAsync(int id)
        {
            Desk? desk = await _context.Desks.Include(d => d.Reservations).FirstOrDefaultAsync(d => d.Id == id);

            if (desk == null)
                return false;

            if (desk.Reservations.Any())
                throw new InvalidOperationException("This desk can't be deleted, as it contains reservations");

            _context.Remove(desk);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
