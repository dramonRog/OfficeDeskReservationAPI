using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeDeskReservation.API.Data;
using OfficeDeskReservation.API.Dtos;
using OfficeDeskReservation.API.Models;

namespace OfficeDeskReservation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DesksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DesksController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<List<DeskDto>>> GetDesksAsync()
        {
            List<Desk> desks = await _context.Desks.ToListAsync();
            List<DeskDto> desksDtos = desks.Select(desk => new DeskDto
            {
                Id = desk.Id,
                DeskIdentifier = desk.DeskIdentifier,
                RoomId = desk.RoomId
            }).ToList();

            return Ok(desksDtos);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<DeskDto>> GetDeskByIdAsync(int id)
        {
            Desk? existingDesk = await _context.Desks.FirstOrDefaultAsync(desk => desk.Id == id);

            if (existingDesk == null)
                return NotFound();

            DeskDto result = new DeskDto
            {
                Id = id,
                DeskIdentifier = existingDesk.DeskIdentifier,
                RoomId = existingDesk.RoomId
            };

            return Ok(result);
        }


        [HttpPost]
        public async Task<ActionResult<DeskDto>> PostDeskAsync([FromBody] DeskDto desk)
        {
            if (await _context.Desks.AnyAsync(d => d.DeskIdentifier == desk.DeskIdentifier))
                return Conflict("The desk with than name already exists.");

            if (!await _context.Rooms.AnyAsync(r => r.Id == desk.RoomId))
                return BadRequest("This room doesn't exist.");

            Desk result = new Desk
            {
                DeskIdentifier = desk.DeskIdentifier,
                RoomId = desk.RoomId
            };

            _context.Desks.Add(result);
            await _context.SaveChangesAsync();
            
            desk.Id = result.Id;
            return CreatedAtAction("GetDeskById", new { id = desk.Id }, desk);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutDeskAsync(int id, [FromBody] DeskDto desk)
        {
            if (id != desk.Id)
                return BadRequest();

            Desk? updatingDesk = await _context.Desks.FirstOrDefaultAsync(d => d.Id == id);
            if (updatingDesk == null)
                return NotFound();

            if (!await _context.Rooms.AnyAsync(r => r.Id == desk.RoomId))
                return BadRequest("The specified RoomId does not exist.");

            updatingDesk.DeskIdentifier = desk.DeskIdentifier;
            updatingDesk.RoomId = desk.RoomId;
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeskByIdAsync(int id)
        {
            Desk? deskToRemove = await _context.Desks.FirstOrDefaultAsync(d => d.Id == id);

            if (deskToRemove == null)
                return NotFound();

            _context.Desks.Remove(deskToRemove);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
