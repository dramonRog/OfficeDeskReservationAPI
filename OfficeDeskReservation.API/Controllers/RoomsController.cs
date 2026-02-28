using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Models;
using System.Runtime.CompilerServices;

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
        public async Task<ActionResult<List<Room>>> GetRoomsAsync()
        {
            List<Room> rooms = await _context.Rooms.Include(r => r.Desks).ToListAsync();
            return Ok(rooms);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoomByIdAsync(int id)
        {
            Room? result = await _context.Rooms
                .Include(r => r.Desks)
                .FirstOrDefaultAsync(r => r.Id == id);

            return result == null ? NotFound() : Ok(result);
        }


        [HttpPost]
        public async Task<ActionResult<Room>> PostRoomAsync([FromBody] Room room)
        {
            if (await _context.Rooms.AnyAsync(r => r.Name == room.Name))
                return Conflict("Room with this name already exists.");

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

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
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] Room room)
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
