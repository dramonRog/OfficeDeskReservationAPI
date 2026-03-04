using Microsoft.AspNetCore.Mvc;
using OfficeDeskReservation.API.Dtos.Users;
using OfficeDeskReservation.API.Services.Interfaces;

namespace OfficeDeskReservation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        public UsersController(IUserService service)
        {
            _service = service;
        }


        [HttpGet]
        public async Task<ActionResult<List<UserResponseDto>>> GetUsersAsync()
        {
            List<UserResponseDto> users = await _service.GetUsersAsync();
            return Ok(users);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUserByIdAsync(int id)
        {
            UserResponseDto? user = await _service.GetUserByIdAsync(id);

            if (user == null)
                return NotFound();
            return Ok(user);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserAsync(int id, [FromBody] UserDto user)
        {
            if (await _service.UpdateUserAsync(id, user))
                return NoContent();
            return NotFound();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveUserByIdAsync(int id)
        {
            if (await _service.DeleteUserAsync(id))
                return NoContent();
            return NotFound();
        }
    }
}
