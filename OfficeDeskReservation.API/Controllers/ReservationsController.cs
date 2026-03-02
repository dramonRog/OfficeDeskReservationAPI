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
        public async Task<ActionResult<List<ReservationResponseDto>>> GetAllReservationsAsync()
        {
            List<Reservation> reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Desk)
                    .ThenInclude(d => d.Room)
                .ToListAsync();

            return Ok(_mapper.Map<List<ReservationResponseDto>>(reservations));
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationResponseDto>> GetReservationByIdAsync(int id)
        {
            Reservation? reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Desk)
                    .ThenInclude(d => d.Room)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound();

            return Ok(_mapper.Map<ReservationResponseDto>(reservation));
        }


        [HttpPost]
        public async Task<ActionResult<ReservationResponseDto>> PostReservationAsync([FromBody] ReservationDto reservation)
        {
            if (reservation.StartTime < DateTime.Now || reservation.StartTime >= reservation.EndTime)
                return BadRequest("Invalid dates. Start time must be in the future and before the end time.");

            User? existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == reservation.UserId);
            if (existingUser == null)
                return NotFound("No user with such ID");
            Desk? existingDesk = await _context.Desks
                .Include(d => d.Room)
                .FirstOrDefaultAsync(d => d.Id == reservation.DeskId);
            if (existingDesk == null)
                return NotFound("No desk with such ID");

            bool isOverlapping = await _context.Reservations.AnyAsync(r =>
                r.DeskId == reservation.DeskId &&
                r.StartTime < reservation.EndTime &&
                r.EndTime > reservation.StartTime
            );

            if (isOverlapping)
                return Conflict("Desk is already taken for the selected period!");

            Reservation newReservation = _mapper.Map<Reservation>(reservation);
            newReservation.User = existingUser;
            newReservation.Desk = existingDesk;

            _context.Reservations.Add(newReservation);
            await _context.SaveChangesAsync();

            ReservationResponseDto responseReservation = _mapper.Map<ReservationResponseDto>(newReservation);

            return CreatedAtAction("GetReservationById", new { id = responseReservation.Id }, responseReservation);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservationAsync(int id, [FromBody] ReservationDto reservation)
        {
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
