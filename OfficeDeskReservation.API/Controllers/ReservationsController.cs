using Microsoft.AspNetCore.Mvc;
using OfficeDeskReservation.API.Dtos.Reservations;
using OfficeDeskReservation.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using OfficeDeskReservation.API.Models;
using OfficeDeskReservation.API.Pagination;

namespace OfficeDeskReservation.API.Controllers
{
    [Authorize]
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
        public async Task<ActionResult<PagedResult<ReservationResponseDto>>> GetAllReservationsAsync([FromQuery] ReservationQueryParameters queryParameters)
        {
            PagedResult<ReservationResponseDto> reservations = await _service.GetAllReservationsAsync(queryParameters);
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
            string? userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized("User ID is missing from the token");

            int currentUserId = int.Parse(userIdString);

            ReservationResponseDto? responseReservation = await _service.CreateReservationAsync(reservation, currentUserId);
            return CreatedAtAction("GetReservationById", new { id = responseReservation?.Id }, responseReservation);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservationAsync(int id, [FromBody] ReservationDto reservation)
        {
            ReservationResponseDto? existingReservation = await _service.GetReservationByIdAsync(id);
            if (existingReservation == null)
                return NotFound();

            int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            string? userRole = User.FindFirstValue(ClaimTypes.Role);

            if (existingReservation.UserId != currentUserId && userRole == Role.User.ToString())
                return Forbid();

            if (await _service.UpdateReservationAsync(id, reservation))
                return NoContent();
            return NotFound();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveReservationAsync(int id)
        {
            ReservationResponseDto? reservation = await _service.GetReservationByIdAsync(id);
            if (reservation == null)
                return NotFound();

            int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            string? userRole = User.FindFirstValue(ClaimTypes.Role);

            if (reservation.UserId != currentUserId && userRole == Role.User.ToString())
                return Forbid();

            await _service.DeleteReservationAsync(id);
            return NoContent();
        }
    }
}
