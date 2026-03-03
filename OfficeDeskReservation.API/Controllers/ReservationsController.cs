using Microsoft.AspNetCore.Mvc;
using OfficeDeskReservation.API.Dtos.Reservations;
using OfficeDeskReservation.API.Services.Interfaces;

namespace OfficeDeskReservation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _service;
        public ReservationsController(IReservationService service)
        {
            _service = service;
        }


        [HttpGet]
        public async Task<ActionResult<List<ReservationResponseDto>>> GetAllReservationsAsync()
        {
            List<ReservationResponseDto> reservations = await _service.GetAllReservationsAsync();
            return Ok(reservations);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationResponseDto>> GetReservationByIdAsync(int id)
        {
            ReservationResponseDto? reservation = await _service.GetReservationByIdAsync(id);

            if (reservation == null)
                return NotFound();

            return Ok(reservation);
        }


        [HttpPost]
        public async Task<ActionResult<ReservationResponseDto>> PostReservationAsync([FromBody] ReservationDto reservation)
        {
            ReservationResponseDto? responseReservation = await _service.CreateReservationAsync(reservation);
            return CreatedAtAction("GetReservationById", new { id = responseReservation?.Id }, responseReservation);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservationAsync(int id, [FromBody] ReservationDto reservation)
        {
            if (await _service.UpdateReservationAsync(id, reservation))
                return NoContent();
            return NotFound();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveReservationAsync(int id)
        {
            if (await _service.DeleteReservationAsync(id))
                return NoContent();
            return NotFound();
        }
    }
}
