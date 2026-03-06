using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Dtos.Reservations;
using OfficeDeskReservation.API.Models;
using OfficeDeskReservation.API.Pagination;
using OfficeDeskReservation.API.Services.Interfaces;

namespace OfficeDeskReservation.API.Services.Implementations
{
    public class ReservationService : IReservationService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ReservationService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedResult<ReservationResponseDto>> GetAllReservationsAsync(QueryParameters queryParameters)
        {
            var query = _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Desk)
                    .ThenInclude(d => d.Room)
                .AsQueryable();

            int totalCount = await query.CountAsync();

            List<Reservation> reservations = await query
                .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .ToListAsync();

            return new PagedResult<ReservationResponseDto>
            {
                Items = _mapper.Map<List<ReservationResponseDto>>(reservations),
                TotalCount = totalCount,
                PageNumber = queryParameters.PageNumber,
                PageSize = queryParameters.PageSize
            };
        }

        public async Task<ReservationResponseDto?> GetReservationByIdAsync(int id)
        {
            Reservation? reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Desk)
                    .ThenInclude(d => d.Room)
                .FirstOrDefaultAsync(r => r.Id == id);

            return reservation == null ? null : _mapper.Map<ReservationResponseDto>(reservation);
        }

        public async Task<ReservationResponseDto?> CreateReservationAsync(ReservationDto reservation, int userId)
        {
            User? existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (existingUser == null)
                throw new KeyNotFoundException("User from token not found in database.");

            Desk? existingDesk = await _context.Desks
                .Include(d => d.Room)
                .FirstOrDefaultAsync(d => d.Id == reservation.DeskId);

            if (existingDesk == null)
                throw new KeyNotFoundException("No desk with such ID.");

            bool isOverlapping = await _context.Reservations.AnyAsync(r =>
                r.DeskId == reservation.DeskId &&
                r.StartTime < reservation.EndTime &&
                r.EndTime > reservation.StartTime
            );

            if (isOverlapping)
                throw new InvalidOperationException("Desk is already taken for that period.");

            Reservation newReservation = _mapper.Map<Reservation>(reservation);
            newReservation.UserId = userId;
            newReservation.User = existingUser;
            newReservation.Desk = existingDesk;

            _context.Reservations.Add(newReservation);
            await _context.SaveChangesAsync();

            return _mapper.Map<ReservationResponseDto>(newReservation);
        }

        public async Task<bool> UpdateReservationAsync(int id, ReservationDto reservation)
        {
            Reservation? existingReservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id);
            if (existingReservation == null)
                return false;

            if (!await _context.Users.AnyAsync(u => u.Id == existingReservation.UserId))
                throw new KeyNotFoundException("No user with such ID.");

            if (!await _context.Desks.AnyAsync(d => d.Id == reservation.DeskId))
                throw new KeyNotFoundException("No desk with such Id.");

            bool isOverlapping = await _context.Reservations.AnyAsync(r =>
                r.Id != id &&
                r.DeskId == reservation.DeskId &&
                r.StartTime < reservation.EndTime &&
                r.EndTime > reservation.StartTime
            );

            if (isOverlapping)
                throw new InvalidOperationException("Desk is already taken for the selected period.");

            _mapper.Map(reservation, existingReservation);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteReservationAsync(int id)
        {
            Reservation? reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return false;

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
