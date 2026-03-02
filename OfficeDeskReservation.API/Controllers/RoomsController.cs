using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Dtos;
using OfficeDeskReservation.API.Models;

namespace OfficeDeskReservation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public RoomsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<ActionResult<List<RoomResponseDto>>> GetRoomsAsync()
        {
            List<Room> rooms = await _context.Rooms.Include(r => r.Desks).ToListAsync();
            List<RoomResponseDto> roomsDtos = _mapper.Map<List<RoomResponseDto>>(rooms);

            return Ok(roomsDtos);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<RoomResponseDto>> GetRoomByIdAsync(int id)
        {
            Room? existingRoom = await _context.Rooms
                .Include(r => r.Desks)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingRoom == null)
                return NotFound();

            RoomResponseDto roomDto = _mapper.Map<RoomResponseDto>(existingRoom);

            return Ok(roomDto);
        }


        [HttpPost]
        public async Task<ActionResult<RoomResponseDto>> PostRoomAsync([FromBody] RoomDto room) 
        {
            if (await _context.Rooms.AnyAsync(r => r.Name == room.Name))
                return Conflict("Room with this name already exists.");

            Room result = _mapper.Map<Room>(room);

            _context.Rooms.Add(result);
            await _context.SaveChangesAsync();

            RoomResponseDto response = _mapper.Map<RoomResponseDto>(result);

            return CreatedAtAction("GetRoomById", new { id = response.Id }, response);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdAsync(int id)
        {
            Room? room = await _context.Rooms.Include(r => r.Desks).FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
                return NotFound();

            if (room.Desks.Any())
                return BadRequest("This room can't be deleted, as it contains desks.");

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoomAsync(int id, [FromBody] RoomDto room)
        {
            Room? existingRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id);

            if (existingRoom == null)
                return NotFound();

            if (await _context.Rooms.AnyAsync(r => r.Name == room.Name && r.Id != existingRoom.Id))
                return Conflict("Room with that name already exists!");

            _mapper.Map(room, existingRoom);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
