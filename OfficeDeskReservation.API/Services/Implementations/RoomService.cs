using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Dtos.Rooms;
using OfficeDeskReservation.API.Models;
using OfficeDeskReservation.API.Services.Interfaces;

namespace OfficeDeskReservation.API.Services.Implementations
{
    public class RoomService : IRoomService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public RoomService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<RoomResponseDto>> GetAllRoomsAsync()
        {
            List<Room> rooms = await _context.Rooms
                .Include(r => r.Desks)
                    .ThenInclude(d => d.Reservations)
                        .ThenInclude(res => res.User)
                .ToListAsync();

            return _mapper.Map<List<RoomResponseDto>>(rooms);
        }

        public async Task<RoomResponseDto?> GetRoomByIdAsync(int id)
        {
            Room? room = await _context.Rooms
                .Include(r => r.Desks)
                    .ThenInclude(d => d.Reservations)
                        .ThenInclude(res => res.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            return room == null ? null : _mapper.Map<RoomResponseDto>(room);
        }

        public async Task<RoomResponseDto?> CreateRoomAsync(RoomDto room)
        {
            if (await _context.Rooms.AnyAsync(r => r.Name == room.Name))
                throw new InvalidOperationException("Room with that name already exists.");

            Room result = _mapper.Map<Room>(room);
            _context.Rooms.Add(result);
            await _context.SaveChangesAsync();

            return _mapper.Map<RoomResponseDto>(result);
        }

        public async Task<bool> UpdateRoomAsync(int id, RoomDto room)
        {
            Room? roomToModify = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id);

            if (roomToModify == null)
                return false;

            if (await _context.Rooms.AnyAsync(r => r.Name == room.Name && r.Id != roomToModify.Id))
                throw new InvalidOperationException("Room with that name already exists.");

            _mapper.Map(room, roomToModify);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteRoomAsync(int id)
        {
            Room? room = await _context.Rooms.Include(r => r.Desks).FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
                return false;

            if (room.Desks.Any())
                throw new InvalidOperationException("This room cannot be deleted, as it contains desks.");

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
