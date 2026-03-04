using Microsoft.AspNetCore.Mvc;
using OfficeDeskReservation.API.Dtos.Desks;
using OfficeDeskReservation.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace OfficeDeskReservation.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DesksController : ControllerBase
    {
        private readonly IDeskService _service;
        public DesksController(IDeskService service)
        {
            _service = service;
        }


        [HttpGet]
        public async Task<ActionResult<List<DeskResponseDto>>> GetDesksAsync()
        {
            List<DeskResponseDto> desksDtos = await _service.GetDesksAsync();
            return Ok(desksDtos);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<DeskResponseDto>> GetDeskByIdAsync(int id)
        {
            DeskResponseDto? existingDesk = await _service.GetDeskByIdAsync(id);

            if (existingDesk == null)
                return NotFound();

            return Ok(existingDesk);
        }


        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<ActionResult<DeskResponseDto>> PostDeskAsync([FromBody] DeskDto desk)
        {
            DeskResponseDto? responseDesk = await _service.CreateDeskAsync(desk);
            return CreatedAtAction("GetDeskById", new { id = responseDesk?.Id }, responseDesk);
        }


        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDeskAsync(int id, [FromBody] DeskDto desk)
        {
            if (await _service.UpdateDeskAsync(id, desk))
                return NoContent();
            return NotFound();
        }


        [Authorize(Roles = "Admin,Manager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeskByIdAsync(int id)
        {
            if (await _service.DeleteDeskAsync(id))
                return NoContent();
            return NotFound();
        }
    }
}
