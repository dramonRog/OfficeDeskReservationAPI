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
    public class ReservationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ReservationsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<ActionResult<List<ReservationDto>>> GetAllReservationsAsync()
        {
            List<Reservation> reservations = await _context.Reservations.ToListAsync();
            return Ok(_mapper.Map<List<ReservationDto>>(reservations));
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationDto>> GetReservationByIdAsync(int id)
        {
            Reservation? reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound();

            return Ok(_mapper.Map<ReservationDto>(reservation));
        }


        [HttpPost]
        public async Task<ActionResult<ReservationDto>> PostReservationAsync([FromBody] ReservationDto reservation)
        {
            if (reservation.StartTime < DateTime.Now || reservation.StartTime >= reservation.EndTime)
                return BadRequest("Invalid dates. Start time must be in the future and before the end time.");

            if (!await _context.Users.AnyAsync(u => u.Id == reservation.UserId))
                return NotFound("No user with such ID");
            if (!await _context.Desks.AnyAsync(d => d.Id == reservation.DeskId))
                return NotFound("No desk with such ID");

            bool isOverlapping = await _context.Reservations.AnyAsync(r =>
                r.DeskId == reservation.DeskId &&
                r.StartTime < reservation.EndTime &&
                r.EndTime > reservation.StartTime
            );

            if (isOverlapping)
                return Conflict("Desk is already taken for the selected period!");

            Reservation newReservation = _mapper.Map<Reservation>(reservation);
            _context.Reservations.Add(newReservation);
            await _context.SaveChangesAsync();

            _mapper.Map(newReservation, reservation);

            return CreatedAtAction("GetReservationById", new { id = reservation.Id }, reservation);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservationAsync(int id, [FromBody] ReservationDto reservation)
        {
            if (id != reservation.Id)
                return BadRequest("URL ID doesn't match with body ID.");

            Reservation? existingReservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id);
            if (existingReservation == null)
                return NotFound("No reservation with such ID.");

            if (!await _context.Users.AnyAsync(u => u.Id == reservation.UserId))
                return NotFound("No user with such ID.");

            if (!await _context.Desks.AnyAsync(d => d.Id == reservation.DeskId))
                return NotFound("No desk with such ID.");

            if (reservation.StartTime < DateTime.Now || reservation.StartTime >= reservation.EndTime)
                return BadRequest("Invalid dates. Start time must be in the future and before the end time.");

            bool isOverlapping = await _context.Reservations.AnyAsync(r =>
                r.Id != id && 
                r.DeskId == reservation.DeskId &&
                reservation.StartTime < r.EndTime &&
                reservation.EndTime > r.StartTime
            );

            if (isOverlapping)
                return Conflict("Desk is already taken for the selected period!");

            _mapper.Map(reservation, existingReservation);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveReservationAsync(int id)
        {
            Reservation? reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound();

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
