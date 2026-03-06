using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeDeskReservation.API.Dtos.Rooms;
using OfficeDeskReservation.API.Pagination;
using OfficeDeskReservation.API.Services.Interfaces;

namespace OfficeDeskReservation.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _service;

        public RoomsController(IRoomService service)
        {
            _service = service;
        }


        [HttpGet]
        public async Task<ActionResult<PagedResult<RoomResponseDto>>> GetRoomsAsync([FromQuery] RoomQueryParameters queryParameters)
        {
            PagedResult<RoomResponseDto> roomsDtos = await _service.GetAllRoomsAsync(queryParameters);
            return Ok(roomsDtos);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<RoomResponseDto>> GetRoomByIdAsync(int id)
        {
            RoomResponseDto? response = await _service.GetRoomByIdAsync(id);

            if (response == null)
                return NotFound();

            return Ok(response);
        }


        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<ActionResult<RoomResponseDto>> PostRoomAsync([FromBody] RoomDto room) 
        {
            RoomResponseDto? response = await _service.CreateRoomAsync(room);
            return CreatedAtAction("GetRoomById", new { id = response.Id }, response);
        }


        [Authorize(Roles = "Admin,Manager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdAsync(int id)
        {
            if (await _service.DeleteRoomAsync(id))
                return NoContent();
            else
                return NotFound("No room with that ID.");
        }


        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoomAsync(int id, [FromBody] RoomDto room)
        {
            if (await _service.UpdateRoomAsync(id, room))
                return NoContent();
            return NotFound("No room with that ID.");
        }
    }
}
