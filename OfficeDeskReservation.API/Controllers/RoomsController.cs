using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Dtos;
using OfficeDeskReservation.API.Mappings;
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
        public async Task<ActionResult<List<RoomDto>>> GetRoomsAsync()
        {
            List<Room> rooms = await _context.Rooms.Include(r => r.Desks).ToListAsync();
            List<RoomDto> roomsDtos = _mapper.Map<List<RoomDto>>(rooms);

            return Ok(roomsDtos);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<RoomDto>> GetRoomByIdAsync(int id)
        {
            Room? existingRoom = await _context.Rooms
                .Include(r => r.Desks)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingRoom == null)
                return NotFound();

            RoomDto roomDto = _mapper.Map<RoomDto>(existingRoom);

            return Ok(roomDto);
        }


        [HttpPost]
        public async Task<ActionResult<RoomDto>> PostRoomAsync([FromBody] RoomDto room)
        {
            if (await _context.Rooms.AnyAsync(r => r.Name == room.Name))
                return Conflict("Room with this name already exists.");

            Room result = _mapper.Map<Room>(room);

            _context.Rooms.Add(result);
            await _context.SaveChangesAsync();
            room.Id = result.Id;

            return CreatedAtAction("GetRoomById", new { id = room.Id }, room);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdAsync(int id)
        {
            Room? room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
                return NotFound();
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] RoomDto room)
        {
            if (id != room.Id)
                return BadRequest("ID in URL does not match ID in body.");

            Room? existingRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id);

            if (existingRoom == null)
                return NotFound();

            existingRoom.Name = room.Name;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
