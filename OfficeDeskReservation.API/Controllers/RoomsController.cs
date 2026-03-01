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

        public RoomsController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<List<RoomDto>>> GetRoomsAsync()
        {
            List<Room> rooms = await _context.Rooms.Include(r => r.Desks).ToListAsync();
            List<RoomDto> resultRoomsList = rooms.Select(r => new RoomDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Desks = r.Desks.Select(d =>
                        new DeskDto 
                        { 
                            Id = d.Id, 
                            DeskIdentifier = d.DeskIdentifier 
                        }).ToList()
                }).ToList();

            return Ok(resultRoomsList);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<RoomDto>> GetRoomByIdAsync(int id)
        {
            Room? existingRoom = await _context.Rooms
                .Include(r => r.Desks)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingRoom == null)
                return NotFound();

            RoomDto result = new RoomDto
            {
                Id = existingRoom.Id,
                Name = existingRoom.Name,
                Desks = existingRoom.Desks
                .Select(d => new DeskDto
                {
                    Id = d.Id,
                    DeskIdentifier = d.DeskIdentifier
                }).ToList()
            };

            return Ok(result);
        }


        [HttpPost]
        public async Task<ActionResult<RoomDto>> PostRoomAsync([FromBody] RoomDto room)
        {
            if (await _context.Rooms.AnyAsync(r => r.Name == room.Name))
                return Conflict("Room with this name already exists.");

            Room result = new Room
            {
                Id = room.Id,
                Name = room.Name,
                Desks = room.Desks.Select(d => new Desk
                {
                    Id = d.Id,
                    DeskIdentifier = d.DeskIdentifier
                }).ToList()
            };

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
