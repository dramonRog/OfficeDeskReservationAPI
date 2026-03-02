using AutoMapper;
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
        private readonly IMapper _mapper;
        public DesksController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<ActionResult<List<DeskResponseDto>>> GetDesksAsync()
        {
            List<Desk> desks = await _context.Desks.Include(d => d.Room).ToListAsync();
            List<DeskResponseDto> desksDtos = _mapper.Map<List<DeskResponseDto>>(desks);

            return Ok(desksDtos);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<DeskResponseDto>> GetDeskByIdAsync(int id)
        {
            Desk? existingDesk = await _context.Desks.Include(d => d.Room).FirstOrDefaultAsync(desk => desk.Id == id);

            if (existingDesk == null)
                return NotFound();

            DeskResponseDto deskDto = _mapper.Map<DeskResponseDto>(existingDesk);
            return Ok(deskDto);
        }


        [HttpPost]
        public async Task<ActionResult<DeskResponseDto>> PostDeskAsync([FromBody] DeskDto desk)
        {
            if (await _context.Desks.AnyAsync(d => d.DeskIdentifier == desk.DeskIdentifier))
                return Conflict("The desk with than name already exists.");

            Room? existingRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == desk.RoomId);
            if (existingRoom == null)
                return BadRequest("This room doesn't exist.");

            Desk result = _mapper.Map<Desk>(desk);
            result.Room = existingRoom;

            _context.Desks.Add(result);
            await _context.SaveChangesAsync();

            DeskResponseDto responseDesk = _mapper.Map<DeskResponseDto>(result);
            return CreatedAtAction("GetDeskById", new { id = responseDesk.Id }, responseDesk);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutDeskAsync(int id, [FromBody] DeskDto desk)
        {
            Desk? updatingDesk = await _context.Desks.FirstOrDefaultAsync(d => d.Id == id);
            if (updatingDesk == null)
                return NotFound();

            if (!await _context.Rooms.AnyAsync(r => r.Id == desk.RoomId))
                return BadRequest("The specified RoomId does not exist.");

            if (await _context.Desks.AnyAsync(d => d.Id != id && d.DeskIdentifier == desk.DeskIdentifier))
                return Conflict("The desk with that name already exists!");

            _mapper.Map(desk, updatingDesk);
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
