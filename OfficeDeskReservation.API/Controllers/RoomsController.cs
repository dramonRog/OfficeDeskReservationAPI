using Microsoft.AspNetCore.Mvc;
using OfficeDeskReservation.API.Dtos.Rooms;
using OfficeDeskReservation.API.Services.Interfaces;

namespace OfficeDeskReservation.API.Controllers
{
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
        public async Task<ActionResult<List<RoomResponseDto>>> GetRoomsAsync()
        {
            List<RoomResponseDto> roomsDtos = await _service.GetAllRoomsAsync();
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


        [HttpPost]
        public async Task<ActionResult<RoomResponseDto>> PostRoomAsync([FromBody] RoomDto room) 
        {
            RoomResponseDto? response = await _service.CreateRoomAsync(room);
            return CreatedAtAction("GetRoomById", new { id = response.Id }, response);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdAsync(int id)
        {
            if (await _service.DeleteRoomAsync(id))
                return NoContent();
            else
                return NotFound("No room with that ID.");
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoomAsync(int id, [FromBody] RoomDto room)
        {
            if (await _service.UpdateRoomAsync(id, room))
                return NoContent();
            return NotFound("No room with that ID.");
        }
    }
}
