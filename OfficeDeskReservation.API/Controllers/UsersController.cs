using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeDeskReservation.API.Dtos.Users;
using OfficeDeskReservation.API.Pagination;
using OfficeDeskReservation.API.Services.Interfaces;

namespace OfficeDeskReservation.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        public UsersController(IUserService service)
        {
            _service = service;
        }


        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<ActionResult<PagedResult<UserResponseDto>>> GetUsersAsync([FromQuery] QueryParameters queryParameters)
        {
            PagedResult<UserResponseDto> users = await _service.GetUsersAsync(queryParameters);
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


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserAsync(int id, [FromBody] UserDto user)
        {
            if (await _service.UpdateUserAsync(id, user))
                return NoContent();
            return NotFound();
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveUserByIdAsync(int id)
        {
            if (await _service.DeleteUserAsync(id))
                return NoContent();
            return NotFound();
        }
    }
}
